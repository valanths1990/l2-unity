using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class World : MonoBehaviour {
    public GameObject player;
    public GameObject playerPrefab;
    public GameObject userPrefab;
    public GameObject npcPrefab;
    public Dictionary<int, Entity> players = new Dictionary<int, Entity>();
    public Dictionary<int, Entity> npcs = new Dictionary<int, Entity>();
    public Dictionary<int, Entity> objects = new Dictionary<int, Entity>();
    public LayerMask groundMask;

    public static World instance;
    public static World GetInstance() {
        return instance;
    }

    void Awake() {
        instance = this;

        playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Player.prefab");
        userPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/User.prefab");
        npcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Npc.prefab");
    }

    public void RemoveObject(int id) {
        Entity transform;
        if(objects.TryGetValue(id, out transform)) {
            players.Remove(id);
            npcs.Remove(id);
            objects.Remove(id);

            Object.Destroy(transform.gameObject);
        }
    }

    public void SpawnPlayer(NetworkIdentity identity, PlayerStatus player) {
        InstantiatePlayer(identity, player);
    }

    public void SpawnNpc(NetworkIdentity identity, NpcStatus npc) {
        InstantiateNpc(identity, npc);
    }

    public void InstantiatePlayerOfflineMode() {
        if(GameStateManager.GetInstance().offlineMode) {
            PlayerEntity entity = playerPrefab.GetComponent<PlayerEntity>();
            entity.Identity.Position = playerPrefab.transform.position;
            InstantiatePlayer(entity.Identity, entity.Status);
        }
    }

    public void InstantiatePlayer(NetworkIdentity identity, PlayerStatus status) {
        identity.SetPosY(getGroundHeight(identity.Position));
        GameObject go = (GameObject)Instantiate(playerPrefab, identity.Position, Quaternion.identity);
        PlayerEntity player = go.GetComponent<PlayerEntity>();
        player.Status = status;
        player.Identity = identity;

        players.Add(identity.Id, player);
        objects.Add(identity.Id, player);

        go.GetComponent<PlayerController>().enabled = true;

        if(!GameStateManager.GetInstance().offlineMode) {
            go.GetComponent<NetworkTransformShare>().enabled = true;
        }
          
        go.transform.name = identity.Name;
        go.SetActive(true);

        CameraController.GetInstance().SetTarget(go);
        CameraController.GetInstance().enabled = true;
    }

    public void InstantiateUser(NetworkIdentity identity, PlayerStatus status) {
        identity.SetPosY(getGroundHeight(identity.Position));
        GameObject go = (GameObject)Instantiate(userPrefab, identity.Position, Quaternion.identity);
        UserEntity player = go.GetComponent<UserEntity>();
        player.Status = status;
        player.Identity = identity;

        players.Add(identity.Id, player);
        objects.Add(identity.Id, player);

        go.GetComponent<NetworkTransformReceive>().enabled = true;

        go.transform.name = identity.Name;
        go.SetActive(true);
    }

    public void InstantiateNpc(NetworkIdentity identity, NpcStatus status) {
        /* Should check at npc id to load right npc name and model */
        identity.SetPosY(getGroundHeight(identity.Position));
        GameObject go = (GameObject)Instantiate(npcPrefab, identity.Position, Quaternion.identity);
        identity.Name = "Dummy";
        NpcEntity npc = go.GetComponent<NpcEntity>();
        npc.Status = status;
        npc.Identity = identity;

        npcs.Add(identity.Id, npc);
        objects.Add(identity.Id, npc);

        go.SetActive(true);
    }

    public float getGroundHeight(Vector3 pos) {
        RaycastHit hit;
        if(Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 1.1f, groundMask)) {
            return hit.point.y;
        }

        return pos.y;
    }

    public void UpdateObjectPosition(int id, Vector3 position) {
        Entity e;
        if(objects.TryGetValue(id, out e)) {
            e.GetComponent<NetworkTransformReceive>().SetNewPosition(position);
        }
    }

    public void UpdateObjectDestination(int id, Vector3 position) {
        Entity e;
        if(objects.TryGetValue(id, out e)) {
            e.GetComponent<NetworkTransformReceive>().SetDestination(position);
           e.GetComponent<NetworkTransformReceive>().LookAt(position);
        }
    }

    public void UpdateObjectRotation(int id, float angle) {
        Entity e;
        if(objects.TryGetValue(id, out e)) {
            e.GetComponent<NetworkTransformReceive>().RotateTo(angle);
        }
    }

    public void UpdateObjectAnimation(int id, int animId, float value) {
        Entity e;
        if(objects.TryGetValue(id, out e)) {
            e.GetComponent<NetworkTransformReceive>().SetAnimationProperty(animId, value);
        }
    }

    public void InflictDamageTo(int sender, int target, byte attackId, int value) {
        Entity senderEntity;
        Entity targetEntity;
        if(objects.TryGetValue(sender, out senderEntity)) {
            if(objects.TryGetValue(target, out targetEntity)) {
                //networkTransform.GetComponentInParent<Entity>().ApplyDamage(sender, attackId, value);
                WorldCombat.GetInstance().ApplyDamage(senderEntity.transform, targetEntity.transform, attackId, value);
            }
        }
    }
}
