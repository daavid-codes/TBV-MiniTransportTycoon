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
                if (trackedObjects[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(trackedObjects[i]);
                }
            }
            trackedObjects.Clear();
        }

        [Test]
        public void Awake_InitializesTilemapsAndCollections()
        {
            var context = CreateContext();
            
            var allTilemaps = GetField<Tilemap[]>(context.Controller, "allTilemaps");
            Assert.IsNotNull(allTilemaps);
            Assert.AreEqual(5, allTilemaps.Length);
            
            var roadCoordinates = GetField<List<Vector3Int>>(context.Controller, "roadCoordinates");
            Assert.IsNotNull(roadCoordinates);
        }

        [Test]
        public void ToggleBuildModeUI_SetsModeToRoadBuild()
        {
            var context = CreateContext();
            
            context.Controller.ToggleBuildModeUI();
            
            Assert.AreEqual(NavigationMode.RoadBuild, GetField<NavigationMode>(context.Controller, "navigationMode"));
            Assert.AreEqual(context.ActiveColor, context.BuildButtonImage.color);
            Assert.AreEqual(context.NormalColor, context.BusStopButtonImage.color);
            Assert.AreEqual(context.NormalColor, context.GarageButtonImage.color);
        }

        [Test]
        public void ToggleBusStopBuildModeUI_SetsModeToStopBuild()
        {
            var context = CreateContext();
            
            context.Controller.ToggleBusStopBuildModeUI();
            
            Assert.AreEqual(NavigationMode.StopBuild, GetField<NavigationMode>(context.Controller, "navigationMode"));
            Assert.AreEqual(context.NormalColor, context.BuildButtonImage.color);
            Assert.AreEqual(context.ActiveColor, context.BusStopButtonImage.color);
        }

        [Test]
        public void ToggleGarageBuildModeUI_SetsModeToGarageBuild()
        {
            var context = CreateContext();
            
            context.Controller.ToggleGarageBuildModeUI();
            
            Assert.AreEqual(NavigationMode.GarageBuild, GetField<NavigationMode>(context.Controller, "navigationMode"));
            Assert.AreEqual(context.ActiveColor, context.GarageButtonImage.color);
        }

        [Test]
        public void ToggleDestroyModeUI_SetsModeToDestroy()
        {
            var context = CreateContext();
            
            context.Controller.ToggleDestroyModeUI();
            
            Assert.AreEqual(NavigationMode.Destroy, GetField<NavigationMode>(context.Controller, "navigationMode"));
        }

        [Test]
        public void SetCameraModeUI_SetsModeToCamera()
        {
            var context = CreateContext();
            SetField(context.Controller, "navigationMode", NavigationMode.RoadBuild);
            
            context.Controller.SetCameraModeUI();
            
            Assert.AreEqual(NavigationMode.Camera, GetField<NavigationMode>(context.Controller, "navigationMode"));
            Assert.AreEqual(context.NormalColor, context.BuildButtonImage.color);
        }

        [Test]
        public void TogglePlaceBusModeUI_TogglesFlagAndSetsCameraMode()
        {
            var context = CreateContext();
            SetField(context.Controller, "navigationMode", NavigationMode.RoadBuild);
            
            context.Controller.TogglePlaceBusModeUI();
            
            Assert.IsTrue(GetField<bool>(context.Controller, "placeBus"));
            Assert.AreEqual(NavigationMode.Camera, GetField<NavigationMode>(context.Controller, "navigationMode"));
            Assert.AreEqual(context.ActiveColor, context.PlaceBusButtonImage.color);
            
            context.Controller.TogglePlaceBusModeUI();
            
            Assert.IsFalse(GetField<bool>(context.Controller, "placeBus"));
            Assert.AreEqual(context.NormalColor, context.PlaceBusButtonImage.color);
        }

        [Test]
        public void CanBuildRoadAt_NoGroundTile_ReturnsFalse()
        {
            var context = CreateContext();
            Vector3Int pos = new Vector3Int(100, 100, 0);
            
            bool result = Invoke<bool>(context.Controller, "CanBuildRoadAt", pos);
            
            Assert.IsFalse(result);
        }

        [Test]
        public void CanBuildRoadAt_RoadAlreadyExists_ReturnsFalse()
        {
            var context = CreateContext();
            Vector3Int pos = Vector3Int.zero;
            Invoke(context.Controller, "PlaceRoad", pos);
            
            bool result = Invoke<bool>(context.Controller, "CanBuildRoadAt", pos);
            
            Assert.IsFalse(result);
        }

        [Test]
        public void CanBuildRoadAt_OccupiedByGarage_ReturnsFalse()
        {
            var context = CreateContext();
            Vector3Int pos = Vector3Int.zero;
            var occupied = GetField<HashSet<Vector3Int>>(context.Controller, "occupiedGarageCells");
            occupied.Add(pos);
            
            bool result = Invoke<bool>(context.Controller, "CanBuildRoadAt", pos);
            
            Assert.IsFalse(result);
        }

        [Test]
        public void CanBuildRoadAt_OccupiedByBusStop_ReturnsFalse()
        {
            var context = CreateContext();
            Vector3Int pos = Vector3Int.zero;
            context.BusStops.SetTile(pos, context.BusStopUpTile);
            
            bool result = Invoke<bool>(context.Controller, "CanBuildRoadAt", pos);
            
            Assert.IsFalse(result);
        }

        [Test]
        public void CanBuildRoadAt_NoAdjacentRoad_ReturnsFalse()
        {
            var context = CreateContext();
            Vector3Int pos = new Vector3Int(5, 5, 0);
            
            bool result = Invoke<bool>(context.Controller, "CanBuildRoadAt", pos);
            
            Assert.IsFalse(result);
        }

        [Test]
        public void CanBuildRoadAt_ValidConditions_ReturnsTrue()
        {
            var context = CreateContext();
            Vector3Int pos = new Vector3Int(5, 5, 0);
            Invoke(context.Controller, "PlaceRoad", pos + Vector3Int.up);
            
            bool result = Invoke<bool>(context.Controller, "CanBuildRoadAt", pos);
            
            Assert.IsTrue(result);
        }

        [Test]
        public void CanBuildBusStopAt_ValidConditions_ReturnsTrue()
        {
            var context = CreateContext();
            Vector3Int pos = new Vector3Int(2, 2, 0);
            Invoke(context.Controller, "PlaceRoad", pos + Vector3Int.right);
            
            bool result = Invoke<bool>(context.Controller, "CanBuildBusStopAt", pos);
            
            Assert.IsTrue(result);
        }

        [Test]
        public void CanBuildBusStopAt_OnExistingRoad_ReturnsFalse()
        {
            var context = CreateContext();
            Vector3Int pos = new Vector3Int(2, 2, 0);
            Invoke(context.Controller, "PlaceRoad", pos);
            
            bool result = Invoke<bool>(context.Controller, "CanBuildBusStopAt", pos);
            
            Assert.IsFalse(result);
        }

        [Test]
        public void GetGarageFootprintCells_ReturnsCorrectFourCells()
        {
            var context = CreateContext();
            Vector3Int origin = new Vector3Int(10, 10, 0);
            
            List<Vector3Int> footprint = Invoke<List<Vector3Int>>(context.Controller, "GetGarageFootprintCells", origin);
            
            Assert.AreEqual(4, footprint.Count);
            Assert.IsTrue(footprint.Contains(origin));
            Assert.IsTrue(footprint.Contains(origin + Vector3Int.right));
            Assert.IsTrue(footprint.Contains(origin + Vector3Int.up));
            Assert.IsTrue(footprint.Contains(origin + Vector3Int.up + Vector3Int.right));
        }

        [Test]
        public void CanBuildGarageAt_MissingGround_ReturnsFalse()
        {
            var context = CreateContext();
            Vector3Int origin = new Vector3Int(99, 99, 0);
            
            bool result = Invoke<bool>(context.Controller, "CanBuildGarageAt", origin);
            
            Assert.IsFalse(result);
        }

        [Test]
        public void CanBuildGarageAt_RoadInFootprint_ReturnsFalse()
        {
            var context = CreateContext();
            Vector3Int origin = Vector3Int.zero;
            Invoke(context.Controller, "PlaceRoad", origin + Vector3Int.right);
            
            bool result = Invoke<bool>(context.Controller, "CanBuildGarageAt", origin);
            
            Assert.IsFalse(result);
        }

        [Test]
        public void CanBuildGarageAt_ValidConditions_ReturnsTrue()
        {
            var context = CreateContext();
            Vector3Int origin = new Vector3Int(2, 2, 0);
            Invoke(context.Controller, "PlaceRoad", origin + Vector3Int.left);
            
            bool result = Invoke<bool>(context.Controller, "CanBuildGarageAt", origin);
            
            Assert.IsTrue(result);
        }

        [Test]
        public void TryGetGarageOriginCell_ValidClick_ReturnsTrueAndOrigin()
        {
            var context = CreateContext();
            Vector3Int origin = new Vector3Int(5, 5, 0);
            context.Garage.SetTile(origin, context.GarageTile);
            
            object[] args = { origin + Vector3Int.up, Vector3Int.zero };
            var method = context.Controller.GetType().GetMethod("TryGetGarageOriginCell", InstanceFlags);
            bool result = (bool)method.Invoke(context.Controller, args);
            
            Assert.IsTrue(result);
            Assert.AreEqual(origin, (Vector3Int)args[1]);
        }

        [Test]
        public void GetRoadNeighborMask_NoNeighbors_ReturnsZero()
        {
            var context = CreateContext();
            Vector3Int pos = Vector3Int.zero;
            
            int mask = Invoke<int>(context.Controller, "GetRoadNeighborMask", pos);
            
            Assert.AreEqual(0, mask);
        }

        [Test]
        public void GetRoadNeighborMask_AllNeighbors_ReturnsFifteen()
        {
            var context = CreateContext();
            Vector3Int pos = Vector3Int.zero;
            Invoke(context.Controller, "PlaceRoad", pos + Vector3Int.up);
            Invoke(context.Controller, "PlaceRoad", pos + Vector3Int.right);
            Invoke(context.Controller, "PlaceRoad", pos + Vector3Int.down);
            Invoke(context.Controller, "PlaceRoad", pos + Vector3Int.left);
            
            int mask = Invoke<int>(context.Controller, "GetRoadNeighborMask", pos);
            
            Assert.AreEqual(15, mask);
        }

        [TestCase(0, "StraightUD")]
        [TestCase(1, "StraightUD")]
        [TestCase(2, "StraightLR")]
        [TestCase(3, "TurnUR")]
        [TestCase(4, "StraightUD")]
        [TestCase(5, "StraightUD")]
        [TestCase(6, "TurnRD")]
        [TestCase(7, "TJunctionURD")]
        [TestCase(8, "StraightLR")]
        [TestCase(9, "TurnLU")]
        [TestCase(10, "StraightLR")]
        [TestCase(11, "TJunctionLUR")]
        [TestCase(12, "TurnDL")]
        [TestCase(13, "TJunctionDLU")]
        [TestCase(14, "TJunctionRDL")]
        [TestCase(15, "Intersection")]
        public void GetRoadTileForMask_ReturnsCorrectTileType(int mask, string expectedName)
        {
            var context = CreateContext();
            
            TileBase tile = Invoke<TileBase>(context.Controller, "GetRoadTileForMask", mask);
            
            Assert.IsNotNull(tile);
            Assert.AreEqual(expectedName, tile.name);
        }

        [Test]
        public void CanPlaceCarAt_IsRoad_ReturnsTrue()
        {
            var context = CreateContext();
            Vector3Int pos = Vector3Int.zero;
            Invoke(context.Controller, "PlaceRoad", pos);
            
            bool result = Invoke<bool>(context.Controller, "CanPlaceCarAt", pos);
            
            Assert.IsTrue(result);
        }

        [Test]
        public void CanPlaceCarAt_IsNotRoad_ReturnsFalse()
        {
            var context = CreateContext();
            Vector3Int pos = Vector3Int.zero;
            
            bool result = Invoke<bool>(context.Controller, "CanPlaceCarAt", pos);
            
            Assert.IsFalse(result);
        }

        [Test]
        public void TryGetClosestRoadTile_OnRoad_ReturnsSameCell()
        {
            var context = CreateContext();
            Vector3Int pos = Vector3Int.zero;
            Invoke(context.Controller, "PlaceRoad", pos);
            
            object[] args = { pos, Vector3Int.zero };
            var method = context.Controller.GetType().GetMethod("TryGetClosestRoadTile", InstanceFlags);
            bool result = (bool)method.Invoke(context.Controller, args);
            
            Assert.IsTrue(result);
            Assert.AreEqual(pos, (Vector3Int)args[1]);
        }

        [Test]
        public void TryGetClosestRoadTile_RadiusSearch_ReturnsClosestCell()
        {
            var context = CreateContext();
            Vector3Int start = Vector3Int.zero;
            Vector3Int roadPos = new Vector3Int(0, 3, 0);
            Invoke(context.Controller, "PlaceRoad", roadPos);
            
            object[] args = { start, Vector3Int.zero };
            var method = context.Controller.GetType().GetMethod("TryGetClosestRoadTile", InstanceFlags);
            bool result = (bool)method.Invoke(context.Controller, args);
            
            Assert.IsTrue(result);
            Assert.AreEqual(roadPos, (Vector3Int)args[1]);
        }

        [Test]
        public void FindRoadPath_ValidRoute_ReturnsFullPath()
        {
            var context = CreateContext();
            Vector3Int start = Vector3Int.zero;
            Vector3Int mid1 = new Vector3Int(1, 0, 0);
            Vector3Int mid2 = new Vector3Int(1, 1, 0);
            Vector3Int end = new Vector3Int(2, 1, 0);
            
            Invoke(context.Controller, "PlaceRoad", start);
            Invoke(context.Controller, "PlaceRoad", mid1);
            Invoke(context.Controller, "PlaceRoad", mid2);
            Invoke(context.Controller, "PlaceRoad", end);
            
            List<Vector3Int> path = Invoke<List<Vector3Int>>(context.Controller, "FindRoadPath", start, end);
            
            Assert.IsNotNull(path);
            Assert.AreEqual(4, path.Count);
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(end, path[3]);
        }

        [Test]
        public void FindRoadPath_NoRoute_ReturnsNull()
        {
            var context = CreateContext();
            Vector3Int start = Vector3Int.zero;
            Vector3Int end = new Vector3Int(5, 5, 0);
            
            Invoke(context.Controller, "PlaceRoad", start);
            Invoke(context.Controller, "PlaceRoad", end);
            
            List<Vector3Int> path = Invoke<List<Vector3Int>>(context.Controller, "FindRoadPath", start, end);
            
            Assert.IsNull(path);
        }

        [Test]
        public void SelectCarStop_FirstSelection_AddsToPending()
        {
            var context = CreateContext();
            Vector3Int stop = Vector3Int.zero;
            context.BusStops.SetTile(stop, context.BusStopUpTile);
            
            Invoke(context.Controller, "SelectCarStop", stop);
            
            var pending = GetField<List<Vector3Int>>(context.Controller, "pendingCarStopSelections");
            Assert.AreEqual(1, pending.Count);
            Assert.AreEqual(stop, pending[0]);
        }

        [Test]
        public void SelectCarStop_SameSelection_Ignored()
        {
            var context = CreateContext();
            Vector3Int stop = Vector3Int.zero;
            context.BusStops.SetTile(stop, context.BusStopUpTile);
            
            Invoke(context.Controller, "SelectCarStop", stop);
            Invoke(context.Controller, "SelectCarStop", stop);
            
            var pending = GetField<List<Vector3Int>>(context.Controller, "pendingCarStopSelections");
            Assert.AreEqual(1, pending.Count);
        }

        [Test]
        public void SelectCarStop_TwoSelections_ClearsPendingList()
        {
            var context = CreateContext();
            Vector3Int stop1 = Vector3Int.zero;
            Vector3Int stop2 = new Vector3Int(2, 0, 0);
            
            context.BusStops.SetTile(stop1, context.BusStopUpTile);
            context.BusStops.SetTile(stop2, context.BusStopUpTile);
            Invoke(context.Controller, "PlaceRoad", stop1 + Vector3Int.up);
            Invoke(context.Controller, "PlaceRoad", stop2 + Vector3Int.up);
            Invoke(context.Controller, "PlaceRoad", new Vector3Int(1, 1, 0));
            
            Invoke(context.Controller, "SelectCarStop", stop1);
            Invoke(context.Controller, "SelectCarStop", stop2);
            
            var pending = GetField<List<Vector3Int>>(context.Controller, "pendingCarStopSelections");
            Assert.AreEqual(0, pending.Count);
        }

        [Test]
        public void GetNearestRoadDirection_FindsCorrectOffset()
        {
            var context = CreateContext();
            Vector3Int cellPos = Vector3Int.zero;
            Invoke(context.Controller, "PlaceRoad", new Vector3Int(-2, 0, 0));
            
            Vector3Int dir = Invoke<Vector3Int>(context.Controller, "GetNearestRoadDirection", cellPos);
            
            Assert.AreEqual(Vector3Int.left, dir);
        }

        [Test]
        public void IsRoadCoordinate_ReturnsTrueForPlacedRoad()
        {
            var context = CreateContext();
            Vector3Int cellPos = new Vector3Int(7, 7, 0);
            
            Invoke(context.Controller, "RegisterRoadCoordinate", cellPos);
            bool result = Invoke<bool>(context.Controller, "IsRoadCoordinate", cellPos);
            
            Assert.IsTrue(result);
        }

        [Test]
        public void HasAdjacentRoad_Footprint_ReturnsTrueIfNextToRoad()
        {
            var context = CreateContext();
            List<Vector3Int> footprint = new List<Vector3Int> { Vector3Int.zero, Vector3Int.right };
            Invoke(context.Controller, "PlaceRoad", Vector3Int.up);
            
            bool result = Invoke<bool>(context.Controller, "HasAdjacentRoad", footprint);
            
            Assert.IsTrue(result);
        }

        private ControllerContext CreateContext()
        {
            GameObject go = Track(new GameObject("GameController"));
            GameController controller = go.AddComponent<GameController>();

            ControllerContext context = new ControllerContext
            {
                Controller = controller,
                Ground = CreateTilemap("Ground"),
                Road = CreateTilemap("Road"),
                BusStops = CreateTilemap("BusStops"),
                Garage = CreateTilemap("Garages"),
                Houses = CreateTilemap("Houses"),
                NormalColor = Color.white,
                ActiveColor = Color.gray,
                BuildButtonImage = CreateImage("BuildBtn"),
                BusStopButtonImage = CreateImage("StopBtn"),
                GarageButtonImage = CreateImage("GarageBtn"),
                PlaceBusButtonImage = CreateImage("PlaceBtn"),
                RoadStraightUpDownTile = CreateTile("StraightUD"),
                RoadStraightLeftRightTile = CreateTile("StraightLR"),
                RoadTurnUpRightTile = CreateTile("TurnUR"),
                RoadTurnRightDownTile = CreateTile("TurnRD"),
                RoadTurnDownLeftTile = CreateTile("TurnDL"),
                RoadTurnLeftUpTile = CreateTile("TurnLU"),
                RoadTJunctionUpRightDownTile = CreateTile("TJunctionURD"),
                RoadTJunctionRightDownLeftTile = CreateTile("TJunctionRDL"),
                RoadTJunctionDownLeftUpTile = CreateTile("TJunctionDLU"),
                RoadTJunctionLeftUpRightTile = CreateTile("TJunctionLUR"),
                RoadIntersectionTile = CreateTile("Intersection"),
                BusStopUpTile = CreateTile("StopUp"),
                BusStopRightTile = CreateTile("StopRight"),
                BusStopDownTile = CreateTile("StopDown"),
                BusStopLeftTile = CreateTile("StopLeft"),
                GarageTile = CreateTile("Garage"),
                BusPrefab = Track(new GameObject("BusPrefab")).AddComponent<Bus>()
            };

            for (int x = -15; x <= 15; x++)
            {
                for (int y = -15; y <= 15; y++)
                {
                    context.Ground.SetTile(new Vector3Int(x, y, 0), CreateTile("Grass"));
                }
            }

            SetField(controller, "groundTilemap", context.Ground);
            SetField(controller, "roadTilemap", context.Road);
            SetField(controller, "busStopTilemap", context.BusStops);
            SetField(controller, "garageTilemap", context.Garage);
            SetField(controller, "housesTilemap", context.Houses);
            
            SetField(controller, "roadStraightUpDownTile", context.RoadStraightUpDownTile);
            SetField(controller, "roadStraightLeftRightTile", context.RoadStraightLeftRightTile);
            SetField(controller, "roadTurnUpRightTile", context.RoadTurnUpRightTile);
            SetField(controller, "roadTurnRightDownTile", context.RoadTurnRightDownTile);
            SetField(controller, "roadTurnDownLeftTile", context.RoadTurnDownLeftTile);
            SetField(controller, "roadTurnLeftUpTile", context.RoadTurnLeftUpTile);
            SetField(controller, "roadTJunctionUpRightDownTile", context.RoadTJunctionUpRightDownTile);
            SetField(controller, "roadTJunctionRightDownLeftTile", context.RoadTJunctionRightDownLeftTile);
            SetField(controller, "roadTJunctionDownLeftUpTile", context.RoadTJunctionDownLeftUpTile);
            SetField(controller, "roadTJunctionLeftUpRightTile", context.RoadTJunctionLeftUpRightTile);
            SetField(controller, "roadIntersectionTile", context.RoadIntersectionTile);
            
            SetField(controller, "busStopUpTile", context.BusStopUpTile);
            SetField(controller, "busStopRightTile", context.BusStopRightTile);
            SetField(controller, "busStopDownTile", context.BusStopDownTile);
            SetField(controller, "busStopLeftTile", context.BusStopLeftTile);
            
            SetField(controller, "garageTile", context.GarageTile);
            SetField(controller, "busPrefab", context.BusPrefab);
            
            SetField(controller, "buildButtonImage", context.BuildButtonImage);
            SetField(controller, "busStopButtonImage", context.BusStopButtonImage);
            SetField(controller, "garageButtonImage", context.GarageButtonImage);
            SetField(controller, "placeBusButtonImage", context.PlaceBusButtonImage);
            
            SetField(controller, "normalColor", context.NormalColor);
            SetField(controller, "activeColor", context.ActiveColor);

            Invoke(controller, "Awake");
            return context;
        }

        private Tilemap CreateTilemap(string name)
        {
            GameObject obj = Track(new GameObject(name));
            obj.AddComponent<Grid>();
            return obj.AddComponent<Tilemap>();
        }

        private Image CreateImage(string name)
        {
            GameObject obj = Track(new GameObject(name, typeof(Image)));
            return obj.GetComponent<Image>();
        }

        private Tile CreateTile(string name)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = name;
            return tile;
        }

        private T Track<T>(T obj) where T : UnityEngine.Object
        {
            trackedObjects.Add(obj);
            return obj;
        }

        private static T GetField<T>(object target, string name)
        {
            return (T)FindField(target.GetType(), name).GetValue(target);
        }

        private static void SetField(object target, string name, object value)
        {
            FindField(target.GetType(), name).SetValue(target, value);
        }

        private static T Invoke<T>(object target, string name, params object[] args)
        {
            return (T)Invoke(target, name, args);
        }

        private static object Invoke(object target, string name, params object[] args)
        {
            return FindMethod(target.GetType(), name, args).Invoke(target, args);
        }

        private static FieldInfo FindField(Type type, string name)
        {
            FieldInfo f = type.GetField(name, InstanceFlags);
            return f ?? (type.BaseType != null ? FindField(type.BaseType, name) : null);
        }

        private static MethodInfo FindMethod(Type type, string name, object[] args)
        {
            MethodInfo m = type.GetMethod(name, InstanceFlags);
            if (m != null) return m;

            MethodInfo[] methods = type.GetMethods(InstanceFlags);
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name == name && methods[i].GetParameters().Length == args.Length)
                {
                    return methods[i];
                }
            }

            return type.BaseType != null ? FindMethod(type.BaseType, name, args) : null;
        }

        private class ControllerContext
        {
            public GameController Controller;
            public Tilemap Ground, Road, BusStops, Garage, Houses;
            public Image BuildButtonImage, BusStopButtonImage, GarageButtonImage, PlaceBusButtonImage;
            public Color NormalColor, ActiveColor;
            public Bus BusPrefab;
            public Tile RoadStraightUpDownTile, RoadStraightLeftRightTile, RoadTurnUpRightTile, RoadTurnRightDownTile, RoadTurnDownLeftTile, RoadTurnLeftUpTile;
            public Tile RoadTJunctionUpRightDownTile, RoadTJunctionRightDownLeftTile, RoadTJunctionDownLeftUpTile, RoadTJunctionLeftUpRightTile, RoadIntersectionTile;
            public Tile BusStopUpTile, BusStopRightTile, BusStopDownTile, BusStopLeftTile, GarageTile;
        }
    }
}