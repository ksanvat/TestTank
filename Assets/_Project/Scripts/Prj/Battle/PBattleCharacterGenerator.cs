using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prj.Character;
using Prj.Character.Item;
using PrjCore.Ext.System;
using PrjCore.Ext.Unity;
using PrjCore.System.AsyncTask;
using PrjCore.Unit;
using PrjCore.Util.System;
using PrjCore.Util.Unity;
using UnityEngine;

namespace Prj.Battle {
    public class PBattleCharacterGenerator : PUnit {
        
        [SerializeField] private Config _enemyGeneratorConfig = new Config();
        private State _enemyGeneratorState = new State();

        private Config cnf => _enemyGeneratorConfig;
        private State st => _enemyGeneratorState;

        public void SetGenerationOptions(Vector2 min, Vector2 max) {
            var center = (min + max) * 0.5f;

            st.EnemyGenerationCenter = center;
            st.EnemyGenerationRadius = (center - min).magnitude + cnf.EnemyPositionOffset;
        }
        
        public void SetOnLoadEnd(Action callback) {
            st.OnLoadEnd = callback;
        }

        public PEnemy GenerateRandomEnemy() {
            if (st.EnemiesConfigs.IsNullOrEmpty()) {
                Debug.LogErrorFormat("Cannot crete unit cause {0} IsNullOrEmpty", nameof(st.EnemiesConfigs));
                return null;
            }

            var enemyConfig = PRandomUtil.Select(st.EnemiesConfigs);
            if (enemyConfig == null) {
                Debug.LogErrorFormat("Cannot crete unit cause null in {0}", nameof(st.EnemiesConfigs));
                return null;
            }

            var enemyUnit = enemyConfig.Prefab.CreateUnit();
            if (enemyUnit == null) {
                Debug.LogErrorFormat("Cannot crete unit from prefab");
                return null;
            }
            
            enemyUnit.HP = enemyConfig.MaxHP;
            enemyUnit.Vulnerability = enemyConfig.Vulnerability;
            enemyUnit.MovementSpeed = enemyConfig.MovementSpeed;
            enemyUnit.RotationSpeed = enemyConfig.RotationSpeed;
            enemyUnit.Damage = enemyConfig.Damage;

            float angle = PRandomUtil.Float(0, PMathUtil.TwoPI); 
            enemyUnit.transform.position = st.EnemyGenerationCenter + PMathUtil.PolarToDecart(st.EnemyGenerationRadius, angle);
            enemyUnit.transform.rotation = Quaternion.Euler(0f, 0f, -angle.AngleToDegree());

            return enemyUnit;
        }

        public PTank GenerateTank() {
            if (st.TankConfig == null) {
                Debug.LogErrorFormat("{0} is null", nameof(st.TankConfig));
                return null;
            }

            var tankUnit = st.TankConfig.Prefab.CreateUnit();
            if (tankUnit == null) {
                Debug.LogErrorFormat("Cannot create {0} from prefab", nameof(tankUnit));
                return null;
            }

            tankUnit.HP = st.TankConfig.MaxHP;
            tankUnit.Vulnerability = st.TankConfig.Vulnerability;
            tankUnit.MovementSpeed = st.TankConfig.MovementSpeed;
            tankUnit.RotationSpeed = st.TankConfig.RotationSpeed;

            foreach (var w in st.TankConfig.Weapons) {
                var weaponUnit = w.Prefab.CreateUnit();
                if (weaponUnit == null) {
                    Debug.LogError("Cannot create weapon unit");
                    continue;
                }

                weaponUnit.Damage = w.Damage;
                weaponUnit.ReloadTime = w.ReloadTime;
                weaponUnit.ProjectileVelocity = w.ProjectileVelocity;

                tankUnit.AddWeapon(weaponUnit);
            }
            
            tankUnit.transform.position = st.EnemyGenerationCenter;
            tankUnit.transform.rotation = Quaternion.identity;
            
            return tankUnit;
        }
        
        public override void OnInit() {
            base.OnInit();

            st.EnemiesConfigs = new List<EnemyConfig>();
            st.LoadingInProcess = 0;
            st.OnLoadEnd = null;

            foreach (string enemyConfigPath in cnf.EnemiesConfigPaths) {
                LoadEnemyConfigJson(enemyConfigPath);
            }
            
            LoadTankConfigJson();
            
            TryCallOnLoadEnd();
        }

        private void LoadEnemyConfigJson(string path) {
            var task = new LoadEnemyConfigTask(
                PFileUtil.BuildPathFromUnix(cnf.EnemiesFolder, path + ".json"),
                result => OnLoadEnemyConfigSuccess(path, result),
                () => OnLoadEnemyConfigFailure(path)
            );
            if (!task.Run()) {
                return;
            }

            st.LoadingInProcess++;
        }

        private void OnLoadEnemyConfigSuccess(string path, EnemyConfig enemyConfig) {
            st.LoadingInProcess--;
            AddLoadedEnemy(path, enemyConfig);
            TryCallOnLoadEnd();
        }

        private void OnLoadEnemyConfigFailure(string path) {
            st.LoadingInProcess--;
            Debug.LogErrorFormat("Cannot load enemy image from {0}", path);
            TryCallOnLoadEnd();
        }

        private void LoadTankConfigJson() {
            var task = new LoadTankConfigTask(
                PFileUtil.BuildPathFromUnix(cnf.TankFolder, cnf.TankConfigPath + ".json"),
                OnLoadTankConfigSuccess,
                OnLoadTankConfigFailure
            );
            if (!task.Run()) {
                return;
            }

            st.LoadingInProcess++;
        }

        private void OnLoadTankConfigSuccess(TankConfig tankConfig) {
            st.LoadingInProcess--;
            AddLoadedTank(tankConfig);
            TryCallOnLoadEnd();
        }

        private void OnLoadTankConfigFailure() {
            Debug.LogError("Cannot load tank config");
            PApplicationUtil.Quit(1);
        }

        private void TryCallOnLoadEnd() {
            if (st.LoadingInProcess > 0) {
                return;
            }
            
            if (st.EnemiesConfigs.IsEmpty()) {
                Debug.LogError("Cannot load any enemy config");
                PApplicationUtil.Quit(1);
                return;
            }
            
            st.OnLoadEnd?.Invoke();
            st.OnLoadEnd = null;
        }

        private void AddLoadedEnemy(string path, EnemyConfig enemyConfig) {
            if (!enemyConfig.IsValid()) {
                Debug.LogErrorFormat("EnemyConfig {0} is invalid", path);
                return;
            }
            
            try {
                enemyConfig.Prefab = cnf.Enemies.First(info => info.Name == enemyConfig.Name).Prefab;
            } catch (InvalidOperationException e) {
                Debug.LogErrorFormat("Cannot find enemy {0} in {1}", enemyConfig.Name, nameof(cnf.Enemies));
                Debug.LogException(e);
                return;
            }

            if (enemyConfig.Prefab == null) {
                Debug.LogError("Enemy prefab is null");
                return;
            }

            st.EnemiesConfigs.Add(enemyConfig);
        }

        private void AddLoadedTank(TankConfig tankConfig) {
            if (!tankConfig.IsValid()) {
                Debug.LogError("Invalid tank config");
                PApplicationUtil.Quit(1);
                return;
            }
            
            tankConfig.Prefab = cnf.TankPrefab;
            if (tankConfig.Prefab == null) {
                Debug.LogError("Tank have no prefab");
                PApplicationUtil.Quit(1);
                return;
            }

            foreach (var weaponConfig in tankConfig.Weapons) {
                try {
                    weaponConfig.Prefab = cnf.TankWeapons.First(info => info.Name == weaponConfig.Name).Prefab;
                } catch (InvalidOperationException e) {
                    Debug.LogErrorFormat("Cannot find tank weapon {0} in {1}", weaponConfig.Name, nameof(cnf.TankWeapons));
                    Debug.LogException(e);
                    PApplicationUtil.Quit(1);
                    return;
                }

                if (weaponConfig.Prefab == null) {
                    Debug.LogErrorFormat("Weapon config {0} have no prefab", weaponConfig.Name);
                    PApplicationUtil.Quit(1);
                    return;
                }
            }
            

            st.TankConfig = tankConfig;
        }
        
        [Serializable]
        private class CharacterConfig {
            public float MaxHP = 0;
            public float Vulnerability = 0;
            public float MovementSpeed = 0;
            public float RotationSpeed = 0;

            public virtual bool IsValid() {
                return MaxHP > 0 &&
                       0 < Vulnerability && Vulnerability < 2 &&
                       MovementSpeed > 0 &&
                       RotationSpeed > 0;
            }
        }

        [Serializable]
        private class EnemyConfig : CharacterConfig {
            
            public string Name = null;
            public float Damage = 0;

            [JsonIgnore] public PEnemy Prefab;

            public override bool IsValid() {
                return base.IsValid() && 
                       Name != null &&
                       Damage > 0;
            }
            
        }

        [Serializable]
        private class TankConfig : CharacterConfig {

            public List<Weapon> Weapons = null;

            [JsonIgnore] public PTank Prefab = null;

            public override bool IsValid() {
                return base.IsValid() && 
                       !Weapons.IsNullOrEmpty() &&
                       Weapons.All(w => w.IsValid());
            }
            
            [Serializable]
            public class Weapon {
                public string Name = null;
                public float Damage = 0;
                public float ReloadTime = 0;
                public float ProjectileVelocity = 0;

                [JsonIgnore] public PTankWeapon Prefab = null;

                public bool IsValid() {
                    return Name != null &&
                           Damage > 0 &&
                           ReloadTime > 0 &&
                           ProjectileVelocity > 0;
                }
            }
        }

        private class LoadCharacterConfigTask<T> : PAsyncTask<T> where T : class {

            private readonly string _path;
            
            protected LoadCharacterConfigTask(string path, Action<T> callbackSuccess, Action callbackFailure) 
                : base(callbackSuccess, callbackFailure) 
            {
                PAssert.IsNotNull(path, nameof(path));

                _path = path;
            }
            
            protected override async Task<T> AsyncTask() {
                return await PFileUtil.ReadJsonFileToEndAsync<T>(_path, CT);
            }
        }
        
        private class LoadEnemyConfigTask : LoadCharacterConfigTask<EnemyConfig> {
            public LoadEnemyConfigTask(string path, Action<EnemyConfig> callbackSuccess, Action callbackFailure) 
                : base(path, callbackSuccess, callbackFailure) { }
        }

        private class LoadTankConfigTask : LoadCharacterConfigTask<TankConfig> {
            public LoadTankConfigTask(string path, Action<TankConfig> callbackSuccess, Action callbackFailure) 
                : base(path, callbackSuccess, callbackFailure) { }
        }

        [Serializable]
        private class EnemyPrefab {
            public string Name = null;
            public PEnemy Prefab = null;
        }

        [Serializable]
        private class WeaponPrefab {
            public string Name = null;
            public PTankWeapon Prefab = null;
        }
        
        [Serializable]
        private class Config {
            public string EnemiesFolder = null;
            public List<string> EnemiesConfigPaths = null;
            public List<EnemyPrefab> Enemies = null;
            public float EnemyPositionOffset = 3f;
            
            public string TankFolder = null;
            public string TankConfigPath = null;
            public PTank TankPrefab = null;
            public List<WeaponPrefab> TankWeapons = null;
        }

        [Serializable]
        private class State {
            public List<EnemyConfig> EnemiesConfigs;
            public TankConfig TankConfig;
            public int LoadingInProcess;
            public Action OnLoadEnd;

            public Vector2 EnemyGenerationCenter;
            public float EnemyGenerationRadius;
        }
        
    }
}