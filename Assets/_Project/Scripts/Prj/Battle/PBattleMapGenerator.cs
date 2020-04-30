using System;
using System.Collections.Generic;
using PrjCore.Ext.Unity;
using PrjCore.Unit;
using PrjCore.Util.System;
using UnityEngine;
using UnityEngine.WSA;

namespace Prj.Battle {
    public class PBattleMapGenerator : PUnit {

        [SerializeField] private Config _mapGeneratorConfig = new Config();

        private Config cnf => _mapGeneratorConfig;

        public GameObject Generate(Vector2 min, Vector2 max) {
            PMathUtil.AssureMinMax(ref min, ref max);

            var tile = cnf.Tile.CreateUnit();
            if (tile == null) {
                Debug.LogErrorFormat("Cannot generate tile from prefab");
                return null;
            }

            tile.transform.position = (min + max) * 0.5f;
            tile.Size = (max - min);
            
            return tile.gameObject;
        }

        public List<GameObject> GenerateMapBox(Vector2 min, Vector2 max) {
            PMathUtil.AssureMinMax(ref min, ref max);

            var center = (min + max) * 0.5f;
            var diff = max - min;
            
            var left = cnf.MapBox.CreateUnit();
            left.transform.position = new Vector3(min.x - 0.5f, center.y);
            left.transform.localScale = new Vector3(1, diff.y, 1);

            var right = cnf.MapBox.CreateUnit();
            right.transform.position = new Vector3(max.x + 0.5f, center.y);
            right.transform.localScale = new Vector3(1, diff.y, 1);

            var top = cnf.MapBox.CreateUnit();
            top.transform.position = new Vector3(center.x, max.y + 0.5f);
            top.transform.localScale = new Vector3(diff.x, 1, 1);

            var bottom = cnf.MapBox.CreateUnit();
            bottom.transform.position = new Vector3(center.x, min.y - 0.5f);
            bottom.transform.localScale = new Vector3(diff.x, 1, 1);

            return new List<GameObject> {left, right, top, bottom};
        }

        [Serializable]
        private class Config {
            public PBattleTile Tile = null;
            public GameObject MapBox = null;
        }
        
    }
}