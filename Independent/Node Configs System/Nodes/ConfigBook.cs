using QuizCanners.Inspect;
using UnityEngine;
using System.Collections.Generic;
using QuizCanners.Utils;
using QuizCanners.Migration;
using System;

namespace QuizCanners.IsItGame.NodeNotes
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + ConfigNodesService.NODE_CONFIGS + "/" + FILE_NAME)]
    public partial class ConfigBook : ScriptableObject, IPEGI, IPEGI_ListInspect, IGotName, ICfg, ISerializationCallbackReceiver
    {
        public const string FILE_NAME = "Node Book";
        public const string KEY_SUFFIX = ".key";

        [SerializeField] private CfgData _encodedNodes;
        [SerializeField] private bool _encoded;
        [SerializeField] private int _freeNodeIndex;

        [NonSerialized] private List<Node> _cachedAllNodes = new List<Node>();

        private Node _rootNode = new Node();
      
        public int Version { get; private set; }

        public void OnNodeTreeChanged()
        {
            Version++;
            _cachedAllNodes = new List<Node>();
            _encoded = false;
        }
      
        public List<Node> GetAllNodes() 
        {
            if (_cachedAllNodes.IsNullOrEmpty()) 
            {
                var node = GetRootNode();
                node.PopulateAllNodes(_cachedAllNodes);
            }

            return _cachedAllNodes;
        }

        private Node GetRootNode() 
        {
            if (_encoded)
            {
                this.Decode(_encodedNodes);
                OnNodeTreeChanged();
            }

            return _rootNode;
        }
    
        public NodesChain this[Node.Reference reff]
        {
            get
            {
                NodesChain c = new NodesChain(this);
                GetRootNode().PopulateChainRecursively(reff, c);
                return c;
            }
        }


        private void RepopulateNodesChain(NodesChain c)
        {
            var last = c.GetReferenceToLastNode();
            c.Chain.Clear();
            GetRootNode().PopulateChainRecursively(last, c);
        }
        #region Encode & Decode 

        public void OnBeforeSerialize()
        {
            if (!_encoded)
            {
                _encodedNodes = Encode().CfgData;
                _encoded = true;
            }
        }

        public void OnAfterDeserialize() { }

        public CfgEncoder Encode() =>new CfgEncoder()
                .Add("n", _rootNode);

        public void DecodeTag(string key, CfgData data)
        {
            switch (key) 
            {
                case "n": _rootNode.Decode(data); break;
            }
        }

        #endregion

        #region Inspector

        private static NodesChain _chain_ForInspector;

 
        public string NameForInspector 
        { 
            get => (this) ? name : "NULL"; 
            set => QcUnity.RenameAsset(this,value); 
        }

        public void Inspect()
        {
            using (_chain_ForInspector = new NodesChain(this))
            {
                if (Singleton.Collector.InspectionWarningIfMissing<ConfigNodesService>().nl())
                    return;

                /*if ("Clear".PegiLabel().ClickConfirm(confirmationTag: "DelBook", toolTip: "This will destroy all the nodes. Are you sure?"))
                {
                    _freeNodeIndex = 0;
                    _rootNode = new Node(this);
                    _encoded = false;

                    OnNodeTreeChanged();
                }*/

                GetRootNode().Nested_Inspect();

                pegi.nl();
            }
        }

        public void InspectInList(ref int edited, int ind)
        {
            using (_chain_ForInspector = new NodesChain(this))
            {
                var tmp = NameForInspector.Replace(KEY_SUFFIX, ""); //name;

                if (pegi.editDelayed(ref tmp))
                    NameForInspector = tmp + KEY_SUFFIX;

                if (icon.Enter.Click())
                    edited = ind;

                this.ClickHighlight();
            }
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(ConfigBook))] internal class NodeBookDrawer : PEGI_Inspector_Override { }

}
