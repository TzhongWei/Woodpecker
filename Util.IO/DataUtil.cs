using Grasshopper;
using System.Collections.Generic;
using System;

using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;

namespace Woodpecker.Animation.Util.IO
{
    internal static class DataUtil
    {
        internal static bool GH_Structure2GHDataTreeIGH_Goo(GH_Structure<IGH_Goo> structure, ref DataTree<IGH_Goo> datatree)
        {
            datatree = new DataTree<IGH_Goo>();
            for(int i = 0; i < structure.Branches.Count; i++)
            {
                datatree.AddRange(structure.Branches[i], new GH_Path(i));
            }
            return datatree.AllData().Any(x => x != null);
        }
        
        internal static bool GH_Structure2GH_DataTree<T1, T2>(GH_Structure<T1> structure, ref DataTree<T2> datatree) where T1 : IGH_Goo
        {
            datatree = new DataTree<T2>();
            for(int i = 0; i < structure.Branches.Count; i++)
            {
                datatree.AddRange(structure.Branches[i].ConvertAll(g => g.ScriptVariable() is T2 value ? value : default), 
                new GH_Path(i));
            }
            return datatree.AllData().Count == 0 && datatree.AllData().Any(x => x != null);
        }
        /// <summary>
        /// Align the structure of Target data tree to Goal data tree. The values in Target will be rearranged according to the structure of Goal, and the values in Goal will be ignored. If Goal has fewer branches or items than Target, the last branch or item will be repeated until the structure matches Target. This method is useful for aligning data trees when the structure of one tree is more suitable for processing, but you want to keep the values from another tree. Note that this method assumes that both Target and Goal are not null and that Goal has at least one branch with at least one item. Otherwise, it will throw an exception.
        /// </summary>
        /// <typeparam name="T1">Any objects</typeparam>
        /// <typeparam name="T2">Any objects</typeparam>
        /// <param name="Target">The data tree as the reference structure</param>
        /// <param name="Goal">The data tree to align to Target</param>
        /// <returns>The aligned data tree</returns>
        /// <exception cref="Exception">Thrown when Target or Goal is null, or when Goal has no data</exception>
        internal static DataTree<T1> AlignDataTree<T1, T2>(DataTree<T2> Target, DataTree<T1> Goal)
        {
            if (Target == null) throw new Exception("Target is null");
            if (Goal == null || Goal.DataCount == 0) throw new Exception("Goal is null");
            var newTree = new DataTree<T1>();
            for (int i = 0; i < Target.BranchCount; i++)
                for (int j = 0; j < Target.Branch(i).Count; j++)
                {
                    var Ei = Goal.BranchCount > i ? i : Goal.BranchCount - 1;
                    if (Goal.Branch(Ei) == null || Goal.Branch(Ei).Count == 0)
                        throw new Exception($"Goal at branch({Ei}) is null");
                    var Ej = Goal.Branch(Ei).Count > j ? j : Goal.Branch(Ei).Count - 1;

                    newTree.Add(Goal.Branch(Ei)[Ej], Target.Path(i));
                }

            return newTree;
        }
        internal static void AlignList<T1, T2>(ref List<T1> list1, ref List<T2> list2)
        {
            if(list1.Count > list2.Count)
            {
                list2 = AlignList(list1, list2);
            }
            else if(list1.Count < list2.Count)
            {
                list1 = AlignList(list2, list1);
            }
        }
        internal static List<T1> AlignList<T1, T2>(List<T2> Target, List<T1> Goal)
        {
            if (Target == null) throw new Exception("Target is null");
            if (Goal == null || Goal.Count == 0) throw new Exception("Goal is null");
            var NewList = new List<T1>();
            for(int i = 0; i < Target.Count; i++)
            {
                var Ei = Goal.Count > i ? i : Goal.Count - 1;
                NewList.Add(Goal[Ei]);
            }

            return NewList;
        }
    }
}