﻿//------------------------------------------------------------
//  Title: LoadOptionsGui
//  Date: 5-20-2014
//  Version: 1.0
//  Project: UAV Swarm
//  Authors: Joshua Christman
//  OS: Windows x64/X86
//  Language:C#
//
//  Class Dependicies: MonoBehaviour
//
//  Description:  Defines a way to manage a real-time population
//--------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTPopulationManager : MonoBehaviour
{
    public GameObject nodePrefab;
    public LoadOptionsGUI loadData;
    string movementBehaviorClassName = "";
    string networkClassBehavior = "";
    int globalCount = 0;
    private object buildMemberLock = new object(); // We need this to prevent many members dying and the count being messed up by threading
    public bool replaceMembers = true; // Can add a toggle later for programs that want to just kill members without replacement

    public Dictionary<GameObject, MemberInfo> populationInfo = new Dictionary<GameObject, MemberInfo>();

    public void initializePopulation(string movementBehaviorClassName, string networkClassBehavior)
    {
        loadData = gameObject.GetComponent<LoadOptionsGUI>();
        this.movementBehaviorClassName = movementBehaviorClassName;
        this.networkClassBehavior = networkClassBehavior;

        for (int i = 0; i < loadData.numNodes; i++)
        {
            GameObject node = buildMemberNode();
            populationInfo.Add(node, new MemberInfo());
        }
        gameObject.GetComponent<LoadOptionsGUI>().paused = false;
    }

    public GameObject buildMemberNode()
    {
        // These next lines will instantiate an game object with the appropriate data
        GameObject node = (GameObject)GameObject.Instantiate(nodePrefab);
        lock (buildMemberLock) // We need to lock on the global node count
        {
            NodeController data = node.GetComponent<NodeController>();

            node.AddComponent(movementBehaviorClassName);

            node.name = "Node " + globalCount;
            node.renderer.material.color = Color.blue;
            data.idNum = globalCount;
            data.idString = "Node " + globalCount;
            data.flightBehavior = (NodeMove)node.GetComponent(movementBehaviorClassName);
         
            if (networkClassBehavior != "none")
            {
                node.AddComponent(networkClassBehavior);
                data.networkBehavior = (Network)node.GetComponent(networkClassBehavior);
                 node.AddComponent<NodeLine>();

            }
            else
                data.networkBehavior = null;

            globalCount++;
        }

        return node;
    }

    public void maintainPopulation()
    {
        if (replaceMembers)
        {
            GameObject newNode = buildMemberNode();
            populationInfo.Add(newNode, new MemberInfo());
            ((IFlightGUIOptions)newNode.GetComponent(movementBehaviorClassName + "GUI")).setSpawnLocation(newNode);
        }
    }

    public bool checkMember(GameObject member)
    {
        MemberInfo info = populationInfo[member];
        if (loadData.maxAge != 0 && loadData.maxAge < info.age)
        {
            populationInfo.Remove(member);
            maintainPopulation();
            return false;
        }
        return true;            
    }
}
