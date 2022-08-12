using NetQuest.UI;
using NetQuest.World;
using Newtonsoft.Json;

namespace NetQuest
{
    public class Game
    {
        private GameWorld? world;

        private Player? player;

        public void Start()
        {
            var saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\NetQuest";
            saveFolder += "\\saves\\";

            var saveFolderInfo = new DirectoryInfo(saveFolder);
            if (!saveFolderInfo.Exists)
            {
                saveFolderInfo.Create();
            }

            List<FileInfo> gameFiles = saveFolderInfo.GetFiles().ToList();

            bool newGame = true;

            if (gameFiles.Count == 0)
            {
                newGame = true;
            }
            else
            {
                Console.WriteLine("Continue or New Game?\n\n1. Continue\n2. New Game");

                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey();
                        if (key.Key == ConsoleKey.D1)
                        {
                            newGame = false;
                            break;
                        }
                        else if (key.Key == ConsoleKey.D2)
                        {
                            newGame = true;
                            break;
                        }
                    }
                }
                Console.Clear();
            }

            if (newGame)
            {
                Console.Write("Enter world name: ");
                string? worldName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(worldName))
                {
                    Console.WriteLine("Invalid world name");
                    Console.ReadLine();
                    return;
                };
                while (worldName is null || gameFiles.Where(x => x.Name == worldName + ".world").Any())
                {
                    Console.Clear();
                    Console.WriteLine("That world name is taken.");
                    Console.WriteLine("Enter world name: ");
                    worldName = Console.ReadLine();
                }
                world = new GameWorld(worldName);
                Room startRoom = new Room();
                startRoom.Name = "Spawn";
                startRoom.Id = 1;
                world.Rooms.Add(1, startRoom);
                world.CurrentRoom = 1;

                Console.Clear();
                Console.WriteLine("You open your eyes to see a dark room.");
                Console.WriteLine("You are surrounded by nothingness.");
                Console.WriteLine("Create.\n");
            }
            else
            {
                int index = 1;
                foreach(var chosenFile in gameFiles)
                {
                    Console.WriteLine($"{index++}. {chosenFile.Name}");
                }
                string? input = Console.ReadLine()?.Trim();
                if(!int.TryParse(input, out index))
                {
                    index = 1;
                }
                using var file = gameFiles[index - 1].OpenRead();
                using var fileReader = new StreamReader(file);
                string fileContents = fileReader.ReadToEnd();
                world = JsonConvert.DeserializeObject<GameWorld>(fileContents);
                Console.Clear();
                Console.WriteLine($"Loaded {file.Name}\n");
            }
        }

        public void Run()
        {
            if (world is null || world.CurrentRoom == 0)
            {
                throw new Exception("World must be initialized.");
            }
            var playerActions = new PlayerActions(world, player);
            var input = new InputParser(Console.ReadLine());
            if(input.ParsedInput.Length == 0)
            {
                return;
            }

            if (input[0] == "look")
            {
                if (input.Length == 1)
                {
                    Console.Write(playerActions.Look());
                }
                else
                {
                    var blueprint = world.Blueprints.Where(x => x.Value.Name == input[1]).FirstOrDefault().Value;
                    if(blueprint is null)
                    {
                        Console.WriteLine("That item doesn't exist.\n");
                        return;
                    }
                    int quantity = world.InventoryContains(blueprint.Name ?? "");
                    if(quantity == 0)
                    {
                        quantity = world.Rooms[world.CurrentRoom].Entities.Where(x => world.Entities[x.Value].Blueprint == blueprint.Id).Any() ? 1 : 0;
                    }
                    if (quantity == 0)
                    {
                        Console.WriteLine("You can't find that anywhere.\n");
                        return;
                    }
                    Console.WriteLine($"{blueprint.Name} ({blueprint.Id})");
                    Console.WriteLine($"{blueprint.Description}");
                    Console.Write($"{(blueprint.Harvestable ? "Harvestable\n" : "")}");
                    Console.WriteLine();
                    return;
                }
            }
            else if (input == "inventory")
            {
                Console.WriteLine("Your inventory contains:");
                if(world.Inventory.Count == 0)
                {
                    Console.WriteLine("Nothing");
                }
                else
                {
                    for(int i = 0; i < world.Inventory.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {world.Entities[world.Inventory[i]].Quantity} {world.Blueprints[world.Entities[world.Inventory[i]].Blueprint].Name}");
                    }
                }
                Console.WriteLine();
            }
            else if (input == "exit")
            {
                throw new Exception("Ending game.");
            }
            else if (input == "clear")
            {
                Console.Clear();
            }
            else if (input[0] == "craft")
            {
                Blueprint? crafting;
                if (int.TryParse(input[1], out int quantity))
                {
                    crafting = world.Blueprints.Where(x => x.Value.Name == input[2]).Select(x => x.Value).FirstOrDefault();
                }
                else
                {
                    quantity = 1;
                    crafting = world.Blueprints.Where(x => x.Value.Name == input[1]).Select(x => x.Value).FirstOrDefault();
                }

                if (crafting is null)
                {
                    Console.WriteLine("There is no blueprint for that!\n");
                    return;
                }
                if(!crafting.Craftable)
                {
                    Console.WriteLine("You aren't able to make that.\n");
                    return;
                }
                bool hasIngredients = true;
                foreach(var ingredient in crafting.Ingredients)
                {

                    var item = world.Inventory.Select(x => world.Entities[x]).Where(x => x.Blueprint == ingredient.Key).FirstOrDefault();
                    if(item is null || item.Quantity < ingredient.Value.quantity * quantity)
                    {
                        hasIngredients = false;
                        break;
                    }
                }
                if(!hasIngredients)
                {
                    Console.WriteLine("You don't have the resources for that.");
                    Console.WriteLine("Required Items:");
                    foreach(var item in crafting.Ingredients)
                    {
                        Console.WriteLine($"{item.Value.quantity} {world.Blueprints[item.Key].Name}{(item.Value.consumed ? "(consumed)" : "")}");
                    }
                    Console.WriteLine();
                    return;
                }
                foreach(var ingredient in crafting.Ingredients)
                {
                    var item = world.Inventory.Select(x => world.Entities[x]).Where(x => x.Blueprint == ingredient.Key).First();
                    if(ingredient.Value.consumed)
                    {
                        item.Quantity -= ingredient.Value.quantity * quantity;
                        if(item.Quantity <= 0)
                        {
                            world.Inventory.Remove(item.Id);
                            world.Entities.Remove(item.Id);
                        }
                    }
                }
                Entity? exists = world.Inventory.Select(x => world.Entities[x]).FirstOrDefault(x => x.Blueprint == crafting.Id);
                if(exists is null)
                {
                    Entity crafted = new();
                    crafted.Id = world.EntityCounter++;
                    crafted.Quantity = quantity;
                    crafted.Blueprint = crafting.Id;
                    crafted.LastHarvest = DateTimeOffset.Now;
                    world.Inventory.Add(crafted.Id);
                    world.Entities[crafted.Id] = crafted;
                }
                else
                {
                    exists.Quantity += quantity;
                }
                Console.WriteLine($"You created {quantity} {crafting.Name}\n");
            }
            else if (input[0] == "harvest")
            {
                if (input.Length == 1)
                {
                    Console.WriteLine("You need to specify what to harvest.\n");
                    return;
                }
                Blueprint? harvestBlueprint = world.Blueprints.Values.FirstOrDefault(x => x.Name == input[input.Length - 1]);
                if(harvestBlueprint is null)
                {
                    Console.WriteLine("That item doesn't exists.\n");
                    return;
                }
                if(!harvestBlueprint.Harvestable)
                {
                    Console.WriteLine("You can't harvest that!\n");
                }
                if (!int.TryParse(input[1], out int quantity))
                {
                    quantity = 1;
                }

                List<Entity> harvestable = new();
                harvestable.AddRange(world.Rooms[world.CurrentRoom].Entities.Where(x => world.Entities[x.Value].Blueprint == harvestBlueprint.Id &&
                                    world.Blueprints[world.Entities[x.Value].Blueprint].HarvestDelay < (DateTime.Now - world.Entities[x.Value].LastHarvest).TotalSeconds).Select(x => world.Entities[x.Value]));
                harvestable.AddRange(world.Inventory.Select(x => world.Entities[x]).Where(x => x.Blueprint == harvestBlueprint.Id &&
                                    world.Blueprints[x.Blueprint].HarvestDelay < (DateTime.Now - x.LastHarvest).TotalSeconds));
                int harvestableQuantity = 0;
                foreach(var harvest in harvestable)
                {
                    harvestableQuantity += harvest.Quantity;
                }

                if(harvestableQuantity <= 0)
                {
                    Console.WriteLine("There are none of those nearby ready to be harvested.\n");
                    return;
                }

                List<Entity> harvested = new();
                bool all = false;
                if (input[1] == "all" || quantity > harvestableQuantity) all = true;

                if (all)
                {
                    quantity = harvestableQuantity;
                    if(!world.HasResouresToHarvestQuantity(harvestBlueprint, harvestableQuantity))
                    {
                        Console.WriteLine("You don't have the resources to harvest that many.\n");
                        return;
                    }
                }
                else
                {
                    if(!world.HasResouresToHarvestQuantity(harvestBlueprint, quantity))
                    {
                        Console.WriteLine("You don't have the resources to harvest that many.\n");
                        return;
                    }
                }

                int totalHarvested = 0;
                while((totalHarvested < quantity || all) && harvestable.Count > 0)
                {
                    Entity harvesting = harvestable[0];
                    harvestable.RemoveAt(0);
                    harvesting.LastHarvest = DateTime.Now;
                    int takeFromHere = Math.Min(quantity - totalHarvested, harvesting.Quantity);
                    totalHarvested += takeFromHere;
                    if(harvestBlueprint.ConsumedOnHarvest)
                    {
                        harvesting.Quantity -= takeFromHere;
                    }
                    harvested.Add(harvesting);
                }

                foreach(var result in harvestBlueprint.HarvestedItems)
                {
                    Entity ent = new Entity();
                    ent.Blueprint = result.Key;
                    ent.Quantity = result.Value * totalHarvested;
                    world.AddToInventory(ent);
                }
                foreach (var ingredient in harvestBlueprint.HarvestRequires.Where(x => x.Value.consumed))
                {
                    world.RemoveFromInventory(world.Blueprints[ingredient.Key], ingredient.Value.quantity);
                }

                foreach(var item in harvested)
                {
                    if(item.Quantity > 0)
                    {
                        continue;
                    }
                    var loc = world.Rooms[world.CurrentRoom].Entities.FirstOrDefault(x => world.Entities[x.Value] == item);
                    if (world.Entities.ContainsKey(loc.Value))
                    {
                        world.Rooms[world.CurrentRoom].Entities.Remove(loc.Key);
                        world.Entities.Remove(world.Entities[loc.Value].Id);
                    }
                    var inv = world.Inventory.Select(x => world.Entities[x]).FirstOrDefault(x => x.Id == item.Id);
                    if(inv is not null)
                    {
                        world.Inventory.Remove(item.Id);
                        world.Entities.Remove(item.Id);
                    }
                }

                Console.Write($"Harvested:\n");
                foreach(var item in harvestBlueprint.HarvestedItems)
                {
                    Console.WriteLine($"{totalHarvested * item.Value} {world.Blueprints[item.Key].Name}");
                }
                Console.WriteLine();
            }
            else if (input[0] == "create")
            {
                if(input.Length is < 2 or > 3)
                {
                    Console.WriteLine("Usage: create [quantity] <id | name>\n");
                    return;
                }
                Blueprint? blueprint;
                if (!int.TryParse(input[1], out int quantity))
                {
                    quantity = 1;
                    blueprint = world.Blueprints.FirstOrDefault(x => x.Value.Name == input[1]).Value;
                }
                else
                {
                    blueprint = world.Blueprints.FirstOrDefault(x => x.Value.Name == input[2]).Value;
                }
               
                if(blueprint is null)
                {
                    Console.WriteLine("That blueprint doesn't exist.\n");
                    return;
                }

                Entity ent = new();
                ent.Blueprint = blueprint.Id;
                ent.Quantity = quantity;
                world.AddToInventory(ent);
                Console.WriteLine($"Created {quantity} {blueprint.Name}\n");
            }
            else if (input[0] == "blueprint")
            {
                if(input.Length == 1)
                {
                    Console.WriteLine("Usage: blueprint <name>\n");
                    return;
                }
                if (input[1] == "list")
                {
                    foreach(var blueprint in world.Blueprints.Values)
                    {
                        Console.WriteLine($"{blueprint.Id} {blueprint.Name}");
                    }
                    Console.WriteLine();
                    return;
                }
                if (UInt64.TryParse(input[1], out UInt64 id) || world.Blueprints.Values.Any(x => x.Name == input[1]))
                {
                    if(id == 0) 
                    {
                        id = world.Blueprints.Values.FirstOrDefault(x => x.Name == input[1])?.Id ?? 0;
                    }
                    if(!world.Blueprints.ContainsKey(id))
                    {
                        Console.WriteLine("That blueprint doesn't exist.\n");
                    }
                    else
                    {
                        var blueprint = world.Blueprints[id];
                        Console.WriteLine($"{blueprint.Name} ({id})\n");
                        Console.WriteLine(blueprint.Description + "\n");
                        Console.WriteLine($"Harvestable: {blueprint.Harvestable}");
                        Console.WriteLine($"Harvest Delay: {blueprint.HarvestDelay} seconds");
                        Console.WriteLine($"Consumed on harvest: {blueprint.ConsumedOnHarvest}");
                        Console.WriteLine("Harvest Rewards:");
                        foreach(var item in blueprint.HarvestedItems)
                        {
                            Console.WriteLine($"{item.Value} {world.Blueprints[item.Key].Name}");
                        }
                        Console.WriteLine("Harvest Requires:");
                        foreach(var item in blueprint.HarvestRequires)
                        {
                            Console.WriteLine($"{item.Value.quantity} {world.Blueprints[item.Key].Name}{(item.Value.consumed ? "(consumed)" : "")}");
                        }
                        Console.WriteLine($"Craftable: {blueprint.Craftable}");
                        Console.WriteLine("Ingredients to craft:");
                        foreach(var item in blueprint.Ingredients)
                        {
                            Console.WriteLine($"{item.Value.quantity} {world.Blueprints[item.Key].Name}{(item.Value.consumed ? "(consumed)" : "")}");
                        }
                        Console.WriteLine();
                    }
                }
                else
                {
                    Blueprint blueprint = new();
                    blueprint.Id = world.BlueprintCounter++;
                    blueprint.Name = input[1];
                    Console.WriteLine("Description:");
                    blueprint.Description = Console.ReadLine();
                    while (true)
                    {
                        Console.WriteLine("Harvestable y or n?");
                        var answer = Console.ReadLine()?.Trim().ToLower();
                        if(answer == "y")
                        {
                            blueprint.Harvestable = true;
                            break;
                        }
                        else if (answer == "n")
                        {
                            blueprint.Harvestable = false;
                            break;
                        }
                    }
                    if(blueprint.Harvestable)
                    {
                        Console.WriteLine("Harvest delay (seconds):");
                        int.TryParse(Console.ReadLine()?.Trim(), out int seconds);
                        blueprint.HarvestDelay = seconds;

                        Console.WriteLine("Harvest requires:");
                        var req = new InputParser(Console.ReadLine() ?? "");
                        if (req.Length != 0)
                        {
                            for(int i = 0; i < req.Length; i+=3)
                            {
                                try
                                {
                                    int quantity = int.Parse(req[i]);
                                    UInt64 blup = world.Blueprints.First(x => x.Value.Name == req[i + 1]).Key;
                                    bool consumed = req[i + 2].ToLower() is "y" or "yes";
                                    blueprint.HarvestRequires.Add(blup, (quantity, consumed));
                                }
                                catch(Exception)
                                {

                                }
                            }
                        }

                        Console.WriteLine("Harvest results:");
                        var res = new InputParser(Console.ReadLine() ?? "");
                        if (res.Length != 0)
                        {
                            for (int i = 0; i < res.Length; i += 2)
                            {
                                try
                                {
                                    int quantity = int.Parse(res[i]);
                                    UInt64 blup = world.Blueprints.First(x => x.Value.Name == res[i + 1]).Key;
                                    blueprint.HarvestedItems.Add(blup, quantity);
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                        Console.WriteLine("Consume on harvest?");
                        blueprint.ConsumedOnHarvest = Console.ReadLine()?.ToLower() is "y" or "yes";
                    }
                    while (true)
                    {
                        Console.WriteLine("Craftable y or n?");
                        var answer = Console.ReadLine()?.Trim().ToLower();
                        if (answer == "y")
                        {
                            blueprint.Craftable = true;
                            break;
                        }
                        else if (answer == "n")
                        {
                            blueprint.Craftable = false;
                            break;
                        }
                    }
                    if (blueprint.Craftable)
                    {
                        Console.WriteLine("Craft requires:");
                        var req = new InputParser(Console.ReadLine() ?? "");
                        if (req[0] != "")
                        {
                            for (int i = 0; i < req.Length; i += 3)
                            {
                                try
                                {
                                    int quantity = int.Parse(req[i]);
                                    UInt64 blup = world.Blueprints.First(x => x.Value.Name == req[i + 1]).Key;
                                    bool consumed = req[i + 2].ToLower() is "y" or "yes";
                                    blueprint.Ingredients.Add(blup, (quantity, consumed));
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                    Console.WriteLine("Blueprint created\n");
                    world.Blueprints[blueprint.Id] = blueprint;
                }
            }
            else if (input[0] == "save")
            {
                Console.WriteLine("Game saved.\n");
            }
            else if (input[0] is "place" or "set" or "drop" or "put")
            {
                if(input.Length < 4)
                {
                    Console.WriteLine("Usage: place <quantity> <blueprint> <location>\n");
                    return;
                }
                int amount = int.Parse(input[1]);
                if(amount > world.InventoryContains(input[2]))
                {
                    amount = world.InventoryContains(input[2]);
                }
                if(amount == 0)
                {
                    Console.WriteLine("You don't have any of that.\n");
                    return;
                }
                var blueprint = world.Blueprints.First(x => x.Value.Name == input[2]).Value;
                var direction = world.Rooms[world.CurrentRoom].GetLocation(input[3]);
                if(world.Rooms[world.CurrentRoom].Entities.ContainsKey(direction) && world.Entities[world.Rooms[world.CurrentRoom].Entities[direction]].Blueprint == blueprint.Id)
                {
                    world.Entities[world.Rooms[world.CurrentRoom].Entities[direction]].Quantity += amount;
                    world.RemoveFromInventory(blueprint, amount);
                    Console.WriteLine($"Added {amount} {blueprint.Name} to the {direction}\n");
                    return;
                }
                else if(!world.Rooms[world.CurrentRoom].Entities.ContainsKey(direction))
                {
                    Entity entity = new();
                    entity.Blueprint = blueprint.Id;
                    entity.Quantity = amount;
                    entity.Id = world.EntityCounter++;
                    world.Rooms[world.CurrentRoom].Entities[direction] = entity.Id;
                    world.Entities.Add(entity.Id, entity);
                    world.RemoveFromInventory(blueprint, amount);
                    Console.WriteLine($"Put {amount} {blueprint.Name} on the {direction}\n");
                    return;
                }
                else
                {
                    Console.WriteLine("There is already something sitting there.\n");
                    return;
                }
                
                
            }
            else if (input[0] is "take" or "grab")
            {
                if(input.Length is < 2 or > 3)
                {
                    Console.WriteLine("Usage: take [quantity] <blueprint>\n");
                    return;
                }

                Blueprint? blueprint;
                List<Entity> entities;
                int quantity;

                try
                {
                    if (input.Length == 3)
                    {
                        quantity = int.Parse(input[1]);
                        blueprint = world.Blueprints.Where(x => x.Value.Name == input[2]).FirstOrDefault().Value;
                    }
                    else
                    {
                        quantity = 1;
                        blueprint = world.Blueprints.Where(x => x.Value.Name == input[1]).FirstOrDefault().Value;
                    }
                    if(blueprint is null)
                    {
                        Console.WriteLine("That item doesn't exist.\n");
                        return;
                    }
                    entities = world.Entities.Values.Where(x => world.Rooms[world.CurrentRoom].Entities.Values.Contains(x.Id) && x.Blueprint == blueprint.Id).ToList();
                    if(entities.Count < 1)
                    {
                        Console.WriteLine("You can't find that in this room.\n");
                        return;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("The taking is all messed up.\n");
                    return;
                }

                List<Entity> modifiedEntities = new();
                int taken = 0;
                while(entities.Count > 0 && taken < quantity)
                {
                    Entity currentEntity = entities.First();
                    entities.Remove(currentEntity);
                    int amountToTake = Math.Min(currentEntity.Quantity, quantity - taken);
                    taken += amountToTake;
                    currentEntity.Quantity -= amountToTake;
                    modifiedEntities.Add(currentEntity);
                }
                {
                    Entity entity = new();
                    entity.Quantity = taken;
                    entity.Blueprint = blueprint.Id;
                    world.AddToInventory(entity);
                }
                foreach(var entity in modifiedEntities)
                {
                    if(entity.Quantity <= 0)
                    {
                        world.Rooms[world.CurrentRoom].Entities.Remove(world.Rooms[world.CurrentRoom].Entities.First(x => x.Value == entity.Id).Key);
                        world.Entities.Remove(entity.Id);
                    }
                }
                Console.WriteLine($"Took {taken} {blueprint.Name}\n");
                
            }
            else
            {
                Console.WriteLine("That input is not supported.\n");
            }

        }
    }
}
