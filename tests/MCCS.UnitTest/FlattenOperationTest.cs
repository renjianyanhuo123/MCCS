using MCCS.UserControl.DynamicGrid.FlattenedGrid;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace MCCS.UnitTest
{
    [TestClass]
    public sealed class FlattenOperationTest
    { 

        [TestMethod] 
        public void BinaryTreeManager_Operation_GetElementDisplacement_Test() 
        { 
            var node1 = new CellLayoutNode();
            var node2 = new CellLayoutNode();
            var node3 = new CellLayoutNode();
            var node4 = new CellLayoutNode();
            var leftSplitterNode = new SplitterNode(CutDirectionEnum.Horizontal, node1, node2)
            {
                Ratio = 0.3
            };
            var rightSplitterNode = new SplitterNode(CutDirectionEnum.Horizontal, node3, node4)
            {
                Ratio = 0.7
            };
            var root = new SplitterNode(CutDirectionEnum.Vertical, leftSplitterNode, rightSplitterNode)
            {
                Ratio = 0.5
            };
            var binaryTreeManager = new BinaryTreeManager(root);
            var res = binaryTreeManager.GetElementDisplacement(root);
            IsTrue(res.ContainsKey(node1.Id));
            IsTrue(res.ContainsKey(node2.Id));
            IsTrue(res.ContainsKey(node3.Id));
            IsTrue(res.ContainsKey(node4.Id));
            IsTrue(res.ContainsKey(leftSplitterNode.Id));
            IsTrue(res.ContainsKey(rightSplitterNode.Id));
            IsTrue(res.ContainsKey(root.Id));
            AreEqual(res[node1.Id].row, 0);
            AreEqual(res[node1.Id].col, 0);
            AreEqual(res[node1.Id].rSpan, 1);
            AreEqual(res[node1.Id].cSpan, 1);
            AreEqual(res[node2.Id].row, 2);
            AreEqual(res[node2.Id].col, 0);
            AreEqual(res[node2.Id].rSpan, 3);
            AreEqual(res[node2.Id].cSpan, 1);
            AreEqual(res[node3.Id].row, 0);
            AreEqual(res[node3.Id].col, 2);
            AreEqual(res[node3.Id].rSpan, 3);
            AreEqual(res[node3.Id].cSpan, 1);
            AreEqual(res[node4.Id].row, 4);
            AreEqual(res[node4.Id].col, 2);
            AreEqual(res[node4.Id].rSpan, 1);
            AreEqual(res[node4.Id].cSpan, 1);
            AreEqual(res[leftSplitterNode.Id].row, 1);
            AreEqual(res[leftSplitterNode.Id].col, 0);
            AreEqual(res[leftSplitterNode.Id].rSpan, 1);
            AreEqual(res[leftSplitterNode.Id].cSpan, 1);
            AreEqual(res[rightSplitterNode.Id].row, 3);
            AreEqual(res[rightSplitterNode.Id].col, 2);
            AreEqual(res[rightSplitterNode.Id].rSpan, 1);
            AreEqual(res[rightSplitterNode.Id].cSpan, 1);
            AreEqual(res[root.Id].row, 0);
            AreEqual(res[root.Id].col, 1);
            AreEqual(res[root.Id].rSpan, 5);
            AreEqual(res[root.Id].cSpan, 1);
        }
    }
}
