using System.IO;
using System.Linq;
using NLog;
using Sandbox.Game.Entities;
using Torch.API;
using VRage.Game.ModAPI;
using VRage.Groups;
using VRage.ObjectBuilders;
using VRageMath;

namespace Torch.Managers
{
    public class EntityManager : Manager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public EntityManager(ITorchBase torch) : base(torch) { }

        public void ExportGrid(IMyCubeGrid grid, string path)
        {
            var ob = grid.GetObjectBuilder(true);
            using (var f = File.Open(path, FileMode.CreateNew))
                MyObjectBuilderSerializer.SerializeXML(f, ob);
        }

        public void ImportGrid(string path, Vector3D position)
        {
            MyObjectBuilder_EntityBase gridOb;
            using (var f = File.OpenRead(path))
                MyObjectBuilderSerializer.DeserializeXML(f, out gridOb);

            var grid = MyEntities.CreateFromObjectBuilderParallel(gridOb);
            grid.PositionComp.SetPosition(position);
            MyEntities.Add(grid);
        }
    }

    public static class GroupExtensions
    {
        public static BoundingBoxD GetWorldAABB(this MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group)
        {
            var grids = group.Nodes.Select(n => n.NodeData);

            var startPos = grids.First().PositionComp.GetPosition();
            var box = new BoundingBoxD(startPos, startPos);

            foreach (var aabb in grids.Select(g => g.PositionComp.WorldAABB))
                box.Include(aabb);

            return box;
        }
    }
}