using System.Collections;
using System.Collections.Generic;
using NabaGame.Core.Runtime.Pool;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BrickEscape
{
    [CreateAssetMenu(fileName = "Pooling", menuName = "GameData/Pooling")]
    public class Pooling : SerializedScriptableObject
    {
        public Race_Avatar avatar;
        public IconEffect iconEffect;
        public GameDot SpawnDot()
        {
            var temp = GameManager.Instance.dataCollection.GetDotPrefab();
            return FastPoolManager.GetPool(temp).FastInstantiate<GameDot>();
        }

        public void DespawnDot(GameDot dot)
        {
            var temp = GameManager.Instance.dataCollection.GetDotPrefab();
            FastPoolManager.GetPool(temp).FastDestroy(dot);
        }
        
        public MoveableBlock SpawnBlock(int blockID)
        {
            var temp = GameManager.Instance.dataCollection.GetBlockByID(blockID);
            return FastPoolManager.GetPool(temp).FastInstantiate<MoveableBlock>();
        }

        public void DespawnBlock(MoveableBlock block, int blockID)
        {
            var temp = GameManager.Instance.dataCollection.GetBlockByID(blockID);
            FastPoolManager.GetPool(temp).FastDestroy(block.gameObject);
        }
        
        public ParticleSystem SpawnEffect(EffectID id)
        {
            var temp = GameManager.Instance.dataCollection.GetEffect(id);
            return FastPoolManager.GetPool(temp).FastInstantiate<ParticleSystem>();
        }

        public void DespawnEffect(EffectID id, ParticleSystem effect)
        {
            var temp = GameManager.Instance.dataCollection.GetEffect(id);
            FastPoolManager.GetPool(temp).FastDestroy(effect.gameObject);
        }

        public ClickEffect SpawnClickEffect(Transform parent)
        {
            var temp = GameManager.Instance.dataCollection.clickEffect;
            return FastPoolManager.GetPool(temp).FastInstantiate<ClickEffect>(parent);
        }
        public void DespawnClickEffect(GameObject go)
        {
            var temp = GameManager.Instance.dataCollection.clickEffect;
            FastPoolManager.GetPool(temp).FastDestroy(go);
        }


        public Race_Avatar SpawnAvatar(Transform parent) 
        {
            return FastPoolManager.GetPool(avatar).FastInstantiate<Race_Avatar>(parent);
        }
        public void DespawnAvatar(GameObject go)
        {
            FastPoolManager.GetPool(avatar).FastDestroy(go);
        }



        public GameObject SpawnTrail()
        {
            var temp = GameManager.Instance.dataCollection.GetTrailObject();
            return FastPoolManager.GetPool(temp).FastInstantiate();
        }
        
        public void DespawnTrail(GameObject trail)
        {
            var temp = GameManager.Instance.dataCollection.GetTrailObject();
            FastPoolManager.GetPool(temp).FastDestroy(trail);
        }

    
        public IconEffect SpawnIconEffect(Transform parent)
        {
            return FastPoolManager.GetPool(iconEffect)
                .FastInstantiate<IconEffect>(Vector2.zero, Quaternion.identity, parent);
        }

        public void DestroyEffect(GameObject destroyedGo)
        {
            FastPoolManager.GetPool(iconEffect).FastDestroy(destroyedGo);
        }




    }
}
