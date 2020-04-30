using System;
using System.Collections;
using System.Collections.Generic;
using Prj.Battle;
using PrjCore.Character;
using PrjCore.Ext.Unity;
using PrjCore.Scene.Controller;
using PrjCore.Unit;
using PrjCore.Util.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Prj.Scene.Controller {
    public class PBattleSceneController : PBaseSceneController {

        [SerializeField] private Config _sceneControllerConfig = new Config();
        private State _sceneControllerState = new State();

        private Config cnf => _sceneControllerConfig;
        private State st => _sceneControllerState;
        
        public override void OnInit() {
            base.OnInit();

            st.ModulesLoading = new HashSet<PUnit>();
            
            st.Camera = cnf.Camera.CreateUnit();
            st.Camera.GetMinMaxWorldPositions(out var worldMinPos, out var worldMaxPos);
            st.Camera.SetGOActive(false);

            st.CameraCanvas.worldCamera = st.Camera.Camera;

            st.CharacterGenerator = cnf.CharacterGenerator.CreateUnit();
            st.ModulesLoading.Add(st.CharacterGenerator);
            st.CharacterGenerator.SetGenerationOptions(worldMinPos, worldMaxPos);
            st.CharacterGenerator.SetOnLoadEnd(OnEnemyGeneratorLoaded);

            st.MapGenerator = cnf.MapGenerator.CreateUnit();
            st.MapGO = st.MapGenerator.Generate(worldMinPos, worldMaxPos);
            st.MapGO.transform.SetParent(st.WorldStaticTransform);
            st.MapBorderGO = st.MapGenerator.GenerateMapBox(worldMinPos, worldMaxPos);
            st.MapBorderGO.ForEach(x => x.transform.SetParent(st.WorldStaticTransform));
            
            st.EnemyUnits = new HashSet<PBaseCharacter>();
        }

        public override void OnDeinit() {
            base.OnDeinit();
            
            StopAllCoroutines();
            
            st.Camera.DestroyUnit();
            st.Camera = null;
            
            st.CharacterGenerator.DestroyUnit();
            st.CharacterGenerator = null;
            
            st.MapGenerator.DestroyUnit();
            st.MapGenerator = null;
            
            st.TankUnit.DestroyUnit();
            st.TankUnit = null;
            
            st.MapBorderGO.ForEach(x => x.DestroyUnit());
            st.MapBorderGO = null;
            
            foreach (var enemyUnit in st.EnemyUnits) {
                enemyUnit.DestroyUnit();
            }
            st.EnemyUnits = null;
        }

        protected void Awake() {
            SetFieldRef(out Transform world, "World");
            SetFieldRef(out st.WorldStaticTransform, world, "Static");
            SetFieldRef(out st.WorldDynamicTransform, world, "Dynamic");
            SetFieldRef(out st.CameraCanvas, "Canvas");
            SetFieldRef(out st.HealthAndScoreText, st.CameraCanvas.transform, "HealthAndScore");
        }

        private void OnEnemyGeneratorLoaded() {
            st.TankUnit = st.CharacterGenerator.GenerateTank();
            st.TankUnit.transform.SetParent(st.WorldDynamicTransform);
            st.TankUnit.SetOnDieCallback(() => {
                Debug.LogFormat("YOU DIED...");
                Debug.LogFormat("Score: {0}", GetScore());
                PApplicationUtil.Quit(0);
            });
            
            OnModuleLoaded(st.CharacterGenerator);
        }
        
        private void OnModuleLoaded(PUnit module) {
            st.ModulesLoading.Remove(module);
            if (st.ModulesLoading.Count <= 0) {
                StartGame();
            }
        }

        private void StartGame() {
            st.Camera.SetGOActive(true);

            StartCoroutine(CreateEnemies());
            StartCoroutine(UpdateHealthAndScore());
        }

        private IEnumerator CreateEnemies() {
            var waitForSeconds = new WaitForSeconds(1);
            
            while (true) {
                if (st.EnemyUnits.Count < cnf.MaxEnemyCount) {
                    var enemyUnit = st.CharacterGenerator.GenerateRandomEnemy();
                    if (enemyUnit == null) {
                        continue;
                    }
                    
                    enemyUnit.transform.SetParent(st.WorldDynamicTransform);
                    enemyUnit.SetOnDieCallback(
                        () => {
                            st.EnemyUnits.Remove(enemyUnit);
                            enemyUnit.DestroyUnit();
                        }
                    );
                    enemyUnit.OpponentGetter = () => st.TankUnit;

                    st.EnemyUnits.Add(enemyUnit);
                }
                
                yield return waitForSeconds;
            }
            
            // ReSharper disable once IteratorNeverReturns
        }

        private IEnumerator UpdateHealthAndScore() {
            while (true) {
                st.HealthAndScoreText.text = $"Health: {st.TankUnit.HP}\nScore: {GetScore()}";
                yield return null;
            }
            
            // ReSharper disable once IteratorNeverReturns
        }

        private int GetScore() {
            return (int) Time.time;
        }
        
        [Serializable]
        private class Config {
            public PBattleCamera Camera = null;
            public PBattleCharacterGenerator CharacterGenerator = null;
            public PBattleMapGenerator MapGenerator = null;

            public int MaxEnemyCount = 10;
        }

        [Serializable]
        private class State {
            public Transform WorldStaticTransform;
            public Transform WorldDynamicTransform;
            public Canvas CameraCanvas;

            public Text HealthAndScoreText;
            
            public PBattleCamera Camera;
            public PBattleCharacterGenerator CharacterGenerator;
            public PBattleMapGenerator MapGenerator;

            public GameObject MapGO;
            public List<GameObject> MapBorderGO;
            
            public HashSet<PUnit> ModulesLoading;

            public PBaseCharacter TankUnit;
            public HashSet<PBaseCharacter> EnemyUnits;
        }
        
    }
}