using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;

namespace QuizCanners.IsItGame.NodeNotes
{

    public partial class ConfigBook
    {
        public class NodesChain : IDisposable, IPEGI, IPEGI_ListInspect, IGotReadOnlyName
        {
            public List<NodeChainToken> Chain = new List<NodeChainToken>();
            public ConfigBook Book { get; private set; }

            public List<NodeChainToken> Nodes
            {
                get
                {
                    if (_version != Book.Version)
                    {
                        _version = Book.Version;
                        Book.RepopulateNodesChain(this);
                    }


                    return Chain;
                }
            }

            public NodesChain GetNodeInChain(int index) 
            {
                var sn = Chain.TryGet(index);
                if (sn == null)
                    return null;

                return Book[new Node.Reference(sn.Node, Book)];
            }

            public Node LastNode 
            {
                get
                {
                    if (Nodes.Count == 0)
                        return null;

                    return Chain[Chain.Count - 1].Node;
                }
            }

            private int _version;

          

            public Node.Reference GetReferenceToLastNode()
            {
                return new Node.Reference(LastNode, Book);
            }

            private bool TryGetConfigFromChain(ITaggedCfg val, out CfgData dta)
            {
                for (int i = Chain.Count - 1; i >= 0; i--)
                    if (Chain[i].Node.TryGetConfig(val, out dta))
                        return true;

                return false;
            }

            public void SaveConfigsOfServicesToChain() 
            {
                List<ITaggedCfg> lstCopy = new List<ITaggedCfg>(Singleton.GetAll<ITaggedCfg>());

                for (int i = Chain.Count - 1; i >= 0; i--)
                {
                    Chain[i].Node.TrySaveCfgAndRemoveFrom(lstCopy);
                    if (lstCopy.Count == 0)
                        break;
                }
            }

            public void LoadConfigsIntoServices()
            {
                foreach(var s in Singleton.GetAll<ITaggedCfg>()) 
                    if (TryGetConfigFromChain(s, out CfgData dta)) 
                        s.Decode(dta);
            }


            public IDisposable AddAndUse(Node node) => new NodeChainToken(node, this);



            public void Dispose()
            {
                Book = null;
            }

            public void Inspect()
            {
                if (LastNode != null)
                {
                    //"Chain: {0} el".F(Chain.Count).nl();

                    using (QcSharp.SetTemporaryValueDisposable(this, val => _chain_ForInspector = val, () => _chain_ForInspector))
                    {
                       // var oldChain = _chain_ForInspector;
                       // _chain_ForInspector = this;
                        LastNode.Inspect(this);
                       // _chain_ForInspector = oldChain;
                    }
                }
                else
                    "Empty Node Chain".PegiLabel().writeWarning();
            }

            public void InspectInList(ref int edited, int index)
            {
                if (LastNode != null)
                    LastNode.InspectInList(ref edited, index);
                else
                    "Empty Node Chain".PegiLabel().writeWarning();
            }

            public string GetReadOnlyName()
            {
                if (LastNode != null)
                    return "Chain [{0}] -> {1}".F(Chain.Count, LastNode.GetNameForInspector());
                else
                    return "Empty Chain";
            }

            public class NodeChainToken : IDisposable
            {
                public Node Node;
                private readonly NodesChain _chain;

                public void Dispose() => _chain.Chain.Remove(this);

                public NodeChainToken(Node node, NodesChain chain)
                {
                    Node = node;
                    _chain = chain;
                    chain.Chain.Add(this);
                }
            }

            public NodesChain(ConfigBook book)
            {
                Book = book;
                _version = book.Version;
            }
        }
    }
}
