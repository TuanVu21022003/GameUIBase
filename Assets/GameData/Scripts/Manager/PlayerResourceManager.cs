using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TW.Utility.CustomType;
using TW.Utility.DesignPattern;
using UnityEngine;
public enum GameBuildType
{
   Release,
   Cheat
}
public class PlayerResourceManager : Singleton<PlayerResourceManager>
{
   public List<GameResourceData> gameResourceData;
   public List<GameResource> gameResource;

   private void Start()
   {
      LoadData();
   }

   private void LoadData()
   {
      gameResourceData = PlayerResourceDataSave.Instance.gameResourceData;
      CreateGameResource();
      var typeCount = Enum.GetValues(typeof(GameResource.Type)).Length;
      if (gameResourceData.Count < typeCount)
      {
         CreateNewResource();
      }
   }

   private void CreateGameResource()
   {
      for (var i = 0; i < gameResourceData.Count; i++)
      {
         var c = gameResourceData[i].C;
         var e = gameResourceData[i].E;
         var oldResource = new GameResource(gameResourceData[i].ResourceType, new BigNumber(c, e));
         gameResource.Add(oldResource);
      }
   }

   private void CreateNewResource()
   {
      var typeCount = Enum.GetValues(typeof(GameResource.Type)).Length;
      for (var i = 0; i < typeCount; i++)
      {
         if (i <= gameResourceData.Count) continue;
         if (i <= 0) continue;
#if UNITY_EDITOR
         Debug.Log("Create new resource: " + (GameResource.Type)i);
#endif
         var newGameResource = new GameResource((GameResource.Type)i, new BigNumber(0));
         gameResource.Add(newGameResource);
         gameResourceData.Add(newGameResource.ToGameResourceData());
      }
      
      PlayerResourceDataSave.Instance.SaveData();
   }

   public GameResource GetGameResource(GameResource.Type type)
   {
      for (var i = 0; i < gameResource.Count; i++)
      {
         if(gameResource[i].ResourceType == type)
         {
            return gameResource[i];
         }
      }

      return null;
   }

   [Button]
   public void ChangeResource(GameResource.Type resourceType, BigNumber amount)
   {
      var resource = GetGameResource(resourceType);
      if (resource == null) return;
      resource.Amount += amount;
      for (var i = 0; i < gameResourceData.Count; i++)
      {
         if (gameResourceData[i].ResourceType == resourceType)
         {
            gameResourceData[i] = resource.ToGameResourceData();
         }
      }
      PlayerResourceDataSave.Instance.SaveData();
   }

   public bool IsEnoughResource(GameResource.Type resourceType, BigNumber amount)
   {
      var resource = GetGameResource(resourceType);
      if (resource == null) return false;
      return resource.Amount >= amount;
   }
}
