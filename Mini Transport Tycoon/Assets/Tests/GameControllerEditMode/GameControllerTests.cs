using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

using MiniTransportTycoon;

namespace MiniTransportTycoon
{
    public class GameControllerTests
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly List<UnityEngine.Object> trackedObjects = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDownTrackedObjects()
        {
            for (int i = trackedObjects.Count - 1; i >= 0; i--)
            {
                UnityEngine.Object trackedObject = trackedObjects[i];
                if (trackedObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(trackedObject);
                }
            }

            trackedObjects.Clear();
        }

        [Test]
        public void NavigationModeUiMethods_SwitchModesAndUpdateBuildButtonColor()
        {
            ControllerContext context = CreateControllerContext();
            Image buildButtonImage = CreateImage("BuildButton");
            Color normalColor = new Color32(10, 20, 30, 255);
            Color activeColor = new Color32(40, 50, 60, 255);

            SetField(context.Controller, "buildButtonImage", buildButtonImage);
            SetField(context.Controller, "normalColor", normalColor);
            SetField(context.Controller, "activeColor", activeColor);

            context.Controller.ToggleBuildModeUI();
            Assert.AreEqual(NavigationMode.RoadBuild, GetField<NavigationMode>(context.Controller, "navigationMode"));
            Assert.AreEqual(activeColor, buildButtonImage.color);

            context.Controller.ToggleBusStopBuildModeUI();
            Assert.AreEqual(NavigationMode.StopBuild, GetField<NavigationMode>(context.Controller, "navigationMode"));
            Assert.AreEqual(normalColor, buildButtonImage.color);

            context.Controller.ToggleGarageBuildModeUI();
            Assert.AreEqual(NavigationMode.GarageBuild, GetField<NavigationMode>(context.Controller, "navigationMode"));

            context.Controller.ToggleDestroyModeUI();
            Assert.AreEqual(NavigationMode.Destroy, GetField<NavigationMode>(context.Controller, "navigationMode"));

            context.Controller.SetCameraModeUI();
            Assert.AreEqual(NavigationMode.Camera, GetField<NavigationMode>(context.Controller, "navigationMode"));
            Assert.AreEqual(normalColor, buildButtonImage.color);
        }

        [Test]
        public void PlaceRoad_RegistersCoordinatesAndUpdatesConnectedTiles()
        {
            ControllerContext context = CreateControllerContext();

            Invoke(context.Controller, "PlaceRoad", Vector3Int.zero);
            Assert.AreSame(context.RoadStraightUpDownTile, context.Road.GetTile(Vector3Int.zero));

            Invoke(context.Controller, "PlaceRoad", Vector3Int.right);
            Assert.AreSame(context.RoadStraightLeftRightTile, context.Road.GetTile(Vector3Int.zero));
            Assert.AreSame(context.RoadStraightLeftRightTile, context.Road.GetTile(Vector3Int.right));

            Invoke(context.Controller, "PlaceRoad", Vector3Int.up);
            Assert.AreSame(context.RoadTurnUpRightTile, context.Road.GetTile(Vector3Int.zero));

            CollectionAssert.AreEquivalent(
                new[] { Vector3Int.zero, Vector3Int.right, Vector3Int.up },
                GetField<List<Vector3Int>>(context.Controller, "roadCoordinates"));
        }

        [Test]
        public void PlaceBusStop_UsesDirectionalTileForAdjacentRoad()
        {
            ControllerContext context = CreateControllerContext();

            Invoke(context.Controller, "PlaceRoad", Vector3Int.right);
            Invoke(context.Controller, "PlaceBusStop", Vector3Int.zero);

            Assert.AreSame(context.BusStopRightTile, context.BusStops.GetTile(Vector3Int.zero));
            Assert.IsFalse(Invoke<bool>(context.Controller, "CanBuildBusStopAt", Vector3Int.zero));
        }

        [Test]
        public void PlaceGarage_OccupiesFootprintAndResolvesOriginFromAnyGarageCell()
        {
            ControllerContext context = CreateControllerContext();

            Invoke(context.Controller, "PlaceRoad", new Vector3Int(-1, 0, 0));
            Invoke(context.Controller, "PlaceGarage", Vector3Int.zero);

            Assert.AreSame(context.GarageTile, context.Garage.GetTile(Vector3Int.zero));

            HashSet<Vector3Int> occupiedCells = GetField<HashSet<Vector3Int>>(context.Controller, "occupiedGarageCells");
            CollectionAssert.IsSubsetOf(
                new[]
                {
                    Vector3Int.zero,
                    Vector3Int.right,
                    Vector3Int.up,
                    Vector3Int.up + Vector3Int.right
                },
                occupiedCells);

            Assert.IsFalse(Invoke<bool>(context.Controller, "CanBuildGarageAt", Vector3Int.zero));

            Assert.IsTrue(InvokeVector3IntOut(context.Controller, "TryGetGarageOriginCell", Vector3Int.up + Vector3Int.right, out Vector3Int garageOrigin));
            Assert.AreEqual(Vector3Int.zero, garageOrigin);
        }

        [Test]
        public void RoadQueries_FindConnectedPathAndClosestReachableRoad()
        {
            ControllerContext context = CreateControllerContext();
            Vector3Int endCell = new Vector3Int(2, 1, 0);

            Invoke(context.Controller, "PlaceRoad", Vector3Int.zero);
            Invoke(context.Controller, "PlaceRoad", Vector3Int.right);
            Invoke(context.Controller, "PlaceRoad", new Vector3Int(2, 0, 0));
            Invoke(context.Controller, "PlaceRoad", endCell);

            List<Vector3Int> path = Invoke<List<Vector3Int>>(context.Controller, "FindRoadPath", Vector3Int.zero, endCell);

            CollectionAssert.AreEqual(
                new[]
                {
                    Vector3Int.zero,
                    Vector3Int.right,
                    new Vector3Int(2, 0, 0),
                    endCell
                },
                path);

            Assert.IsTrue(InvokeVector3IntOut(context.Controller, "TryGetClosestRoadTile", new Vector3Int(0, 2, 0), out Vector3Int closestRoadCell));
            Assert.AreEqual(Vector3Int.zero, closestRoadCell);
        }

        [Test]
        public void SelectCarStop_FinalizesLoopWhenFirstPointIsClickedAgain()
        {
            ControllerContext context = CreateControllerContext();
            Bus busPrefab = CreateBusPrefab();
            Vector3Int firstStopCell = new Vector3Int(0, 1, 0);
            Vector3Int secondStopCell = new Vector3Int(2, 1, 0);

            SetField(context.Controller, "busPrefab", busPrefab);

            Invoke(context.Controller, "PlaceRoad", Vector3Int.zero);
            Invoke(context.Controller, "PlaceRoad", Vector3Int.right);
            Invoke(context.Controller, "PlaceRoad", new Vector3Int(2, 0, 0));

            context.BusStops.SetTile(firstStopCell, context.BusStopDownTile);
            context.BusStops.SetTile(secondStopCell, context.BusStopDownTile);

            Invoke(context.Controller, "SelectCarStop", firstStopCell);
            Assert.AreEqual(1, GetField<List<Vector3Int>>(context.Controller, "pendingCarStopSelections").Count);

            Invoke(context.Controller, "SelectCarStop", secondStopCell);

            Assert.AreEqual(2, GetField<List<Vector3Int>>(context.Controller, "pendingCarStopSelections").Count);

            Invoke(context.Controller, "SelectCarStop", firstStopCell);

            Assert.AreEqual(0, GetField<List<Vector3Int>>(context.Controller, "pendingCarStopSelections").Count);

            Bus spawnedBus = FindSpawnedBus(busPrefab);
            Assert.IsNotNull(spawnedBus);
            Track(spawnedBus.gameObject);

            CollectionAssert.AreEqual(new[] { firstStopCell, secondStopCell }, spawnedBus.StopRoute);
            CollectionAssert.AreEqual(
                new[]
                {
                    Vector3Int.right,
                    new Vector3Int(2, 0, 0)
                },
                spawnedBus.Route);
            Assert.AreEqual(context.Road.GetCellCenterWorld(Vector3Int.zero), spawnedBus.transform.position);
        }

        [Test]
        public void SelectCarStop_AllowsGarageAsRoutePointInLoop()
        {
            ControllerContext context = CreateControllerContext();
            Bus busPrefab = CreateBusPrefab();
            Vector3Int firstStopCell = new Vector3Int(0, 1, 0);
            Vector3Int garageOriginCell = new Vector3Int(2, 2, 0);
            Vector3Int thirdStopCell = new Vector3Int(4, 1, 0);

            SetField(context.Controller, "busPrefab", busPrefab);

            Invoke(context.Controller, "PlaceRoad", Vector3Int.zero);
            Invoke(context.Controller, "PlaceRoad", Vector3Int.right);
            Invoke(context.Controller, "PlaceRoad", new Vector3Int(2, 0, 0));
            Invoke(context.Controller, "PlaceRoad", new Vector3Int(3, 0, 0));
            Invoke(context.Controller, "PlaceRoad", new Vector3Int(4, 0, 0));

            context.BusStops.SetTile(firstStopCell, context.BusStopDownTile);
            context.BusStops.SetTile(thirdStopCell, context.BusStopDownTile);
            Invoke(context.Controller, "PlaceGarage", garageOriginCell);

            Invoke(context.Controller, "SelectCarStop", firstStopCell);
            Invoke(context.Controller, "SelectCarStop", garageOriginCell);
            Invoke(context.Controller, "SelectCarStop", thirdStopCell);
            Invoke(context.Controller, "SelectCarStop", firstStopCell);

            Bus spawnedBus = FindSpawnedBus(busPrefab);
            Assert.IsNotNull(spawnedBus);
            Track(spawnedBus.gameObject);

            CollectionAssert.AreEqual(new[] { firstStopCell, garageOriginCell, thirdStopCell }, spawnedBus.StopRoute);
            CollectionAssert.AreEqual(
                new[]
                {
                    Vector3Int.right,
                    new Vector3Int(2, 0, 0)
                },
                spawnedBus.Route);
            Assert.AreEqual(context.Road.GetCellCenterWorld(Vector3Int.zero), spawnedBus.transform.position);
        }

        [Test]
        public void OnDisable_ClearsPreviewCellsAndPendingStopSelections()
        {
            ControllerContext context = CreateControllerContext();
            List<Vector3Int> previewCells = new List<Vector3Int> { Vector3Int.zero, Vector3Int.right };

            Invoke(context.Controller, "ApplyBuildPreview", previewCells);
            GetField<List<Vector3Int>>(context.Controller, "pendingCarStopSelections").Add(Vector3Int.up);

            Assert.AreNotEqual(Color.white, context.Ground.GetColor(Vector3Int.zero));

            Invoke(context.Controller, "OnDisable");

            Assert.AreEqual(Color.white, context.Ground.GetColor(Vector3Int.zero));
            Assert.AreEqual(Color.white, context.Ground.GetColor(Vector3Int.right));
            Assert.AreEqual(0, GetField<List<Vector3Int>>(context.Controller, "pendingCarStopSelections").Count);
            Assert.AreEqual(0, GetField<List<Vector3Int>>(context.Controller, "previewedBuildCells").Count);
        }

        private ControllerContext CreateControllerContext()
        {
            Grid grid = CreateGrid();
            Tilemap groundTilemap = CreateTilemap("Ground", grid.transform);
            Tilemap roadTilemap = CreateTilemap("Road", grid.transform);
            Tilemap busStopTilemap = CreateTilemap("BusStops", grid.transform);
            Tilemap garageTilemap = CreateTilemap("Garages", grid.transform);
            Tile groundTile = CreateTile("GroundTile");

            FillRect(groundTilemap, -3, 4, -1, 4, groundTile);

            ControllerContext context = new ControllerContext
            {
                Ground = groundTilemap,
                Road = roadTilemap,
                BusStops = busStopTilemap,
                Garage = garageTilemap,
                RoadStraightUpDownTile = CreateTile("RoadStraightUpDown"),
                RoadStraightLeftRightTile = CreateTile("RoadStraightLeftRight"),
                RoadTurnUpRightTile = CreateTile("RoadTurnUpRight"),
                RoadTurnRightDownTile = CreateTile("RoadTurnRightDown"),
                RoadTurnDownLeftTile = CreateTile("RoadTurnDownLeft"),
                RoadTurnLeftUpTile = CreateTile("RoadTurnLeftUp"),
                RoadTJunctionUpRightDownTile = CreateTile("RoadTJunctionUpRightDown"),
                RoadTJunctionRightDownLeftTile = CreateTile("RoadTJunctionRightDownLeft"),
                RoadTJunctionDownLeftUpTile = CreateTile("RoadTJunctionDownLeftUp"),
                RoadTJunctionLeftUpRightTile = CreateTile("RoadTJunctionLeftUpRight"),
                RoadIntersectionTile = CreateTile("RoadIntersection"),
                BusStopUpTile = CreateTile("BusStopUp"),
                BusStopRightTile = CreateTile("BusStopRight"),
                BusStopDownTile = CreateTile("BusStopDown"),
                BusStopLeftTile = CreateTile("BusStopLeft"),
                GarageTile = CreateTile("GarageTile")
            };

            GameObject controllerObject = Track(new GameObject("GameController"));
            context.Controller = controllerObject.AddComponent<GameController>();

            SetField(context.Controller, "groundTilemap", context.Ground);
            SetField(context.Controller, "roadTilemap", context.Road);
            SetField(context.Controller, "busStopTilemap", context.BusStops);
            SetField(context.Controller, "garageTilemap", context.Garage);
            SetField(context.Controller, "roadStraightUpDownTile", context.RoadStraightUpDownTile);
            SetField(context.Controller, "roadStraightLeftRightTile", context.RoadStraightLeftRightTile);
            SetField(context.Controller, "roadTurnUpRightTile", context.RoadTurnUpRightTile);
            SetField(context.Controller, "roadTurnRightDownTile", context.RoadTurnRightDownTile);
            SetField(context.Controller, "roadTurnDownLeftTile", context.RoadTurnDownLeftTile);
            SetField(context.Controller, "roadTurnLeftUpTile", context.RoadTurnLeftUpTile);
            SetField(context.Controller, "roadTJunctionUpRightDownTile", context.RoadTJunctionUpRightDownTile);
            SetField(context.Controller, "roadTJunctionRightDownLeftTile", context.RoadTJunctionRightDownLeftTile);
            SetField(context.Controller, "roadTJunctionDownLeftUpTile", context.RoadTJunctionDownLeftUpTile);
            SetField(context.Controller, "roadTJunctionLeftUpRightTile", context.RoadTJunctionLeftUpRightTile);
            SetField(context.Controller, "roadIntersectionTile", context.RoadIntersectionTile);
            SetField(context.Controller, "busStopUpTile", context.BusStopUpTile);
            SetField(context.Controller, "busStopRightTile", context.BusStopRightTile);
            SetField(context.Controller, "busStopDownTile", context.BusStopDownTile);
            SetField(context.Controller, "busStopLeftTile", context.BusStopLeftTile);
            SetField(context.Controller, "garageTile", context.GarageTile);

            Invoke(context.Controller, "Awake");
            return context;
        }

        private Bus CreateBusPrefab()
        {
            GameObject prefabObject = Track(new GameObject("BusPrefab", typeof(SpriteRenderer)));
            return prefabObject.AddComponent<Bus>();
        }

        private T Track<T>(T obj) where T : UnityEngine.Object
        {
            if (obj != null)
            {
                trackedObjects.Add(obj);
            }

            return obj;
        }

        private Grid CreateGrid()
        {
            GameObject gridObject = Track(new GameObject("Grid", typeof(Grid)));
            return gridObject.GetComponent<Grid>();
        }

        private Tilemap CreateTilemap(string name, Transform parent)
        {
            GameObject tilemapObject = Track(new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer)));
            tilemapObject.transform.SetParent(parent, false);
            return tilemapObject.GetComponent<Tilemap>();
        }

        private Tile CreateTile(string name)
        {
            Tile tile = Track(ScriptableObject.CreateInstance<Tile>());
            tile.name = name;
            return tile;
        }

        private Image CreateImage(string name)
        {
            GameObject imageObject = Track(new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)));
            return imageObject.GetComponent<Image>();
        }

        private void FillRect(Tilemap tilemap, int minX, int maxX, int minY, int maxY, TileBase tile)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        private Bus FindSpawnedBus(Bus prefab)
        {
            Bus[] buses = UnityEngine.Object.FindObjectsByType<Bus>(FindObjectsSortMode.None);
            for (int i = 0; i < buses.Length; i++)
            {
                if (buses[i] != prefab)
                {
                    return buses[i];
                }
            }

            return null;
        }

        private static T GetField<T>(object target, string fieldName)
        {
            return (T)FindField(target.GetType(), fieldName).GetValue(target);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FindField(target.GetType(), fieldName).SetValue(target, value);
        }

        private static T Invoke<T>(object target, string methodName, params object[] args)
        {
            return (T)Invoke(target, methodName, args);
        }

        private static object Invoke(object target, string methodName, params object[] args)
        {
            MethodInfo method = FindMethod(target.GetType(), methodName, args ?? Array.Empty<object>());
            return method.Invoke(target, args);
        }

        private static bool InvokeVector3IntOut(object target, string methodName, Vector3Int input, out Vector3Int output)
        {
            object[] args = { input, null };
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                InstanceFlags,
                null,
                new[] { typeof(Vector3Int), typeof(Vector3Int).MakeByRefType() },
                null);

            Assert.IsNotNull(method, $"Could not find method '{methodName}'.");

            bool result = (bool)method.Invoke(target, args);
            output = (Vector3Int)args[1];
            return result;
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            Type currentType = type;
            while (currentType != null)
            {
                FieldInfo field = currentType.GetField(fieldName, InstanceFlags);
                if (field != null)
                {
                    return field;
                }

                currentType = currentType.BaseType;
            }

            throw new MissingFieldException(type.FullName, fieldName);
        }

        private static MethodInfo FindMethod(Type type, string methodName, object[] args)
        {
            Type currentType = type;
            while (currentType != null)
            {
                MethodInfo[] methods = currentType.GetMethods(InstanceFlags);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo method = methods[i];
                    if (method.Name != methodName)
                    {
                        continue;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length != args.Length)
                    {
                        continue;
                    }

                    bool matches = true;
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        object arg = args[j];
                        Type parameterType = parameters[j].ParameterType;

                        if (arg == null)
                        {
                            if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) == null)
                            {
                                matches = false;
                                break;
                            }

                            continue;
                        }

                        if (!parameterType.IsInstanceOfType(arg))
                        {
                            matches = false;
                            break;
                        }
                    }

                    if (matches)
                    {
                        return method;
                    }
                }

                currentType = currentType.BaseType;
            }

            throw new MissingMethodException(type.FullName, methodName);
        }

        private sealed class ControllerContext
        {
            public GameController Controller;
            public Tilemap Ground;
            public Tilemap Road;
            public Tilemap BusStops;
            public Tilemap Garage;
            public Tile RoadStraightUpDownTile;
            public Tile RoadStraightLeftRightTile;
            public Tile RoadTurnUpRightTile;
            public Tile RoadTurnRightDownTile;
            public Tile RoadTurnDownLeftTile;
            public Tile RoadTurnLeftUpTile;
            public Tile RoadTJunctionUpRightDownTile;
            public Tile RoadTJunctionRightDownLeftTile;
            public Tile RoadTJunctionDownLeftUpTile;
            public Tile RoadTJunctionLeftUpRightTile;
            public Tile RoadIntersectionTile;
            public Tile BusStopUpTile;
            public Tile BusStopRightTile;
            public Tile BusStopDownTile;
            public Tile BusStopLeftTile;
            public Tile GarageTile;
        }
    }
}