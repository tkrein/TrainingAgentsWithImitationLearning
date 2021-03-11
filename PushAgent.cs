using System.Collections;
using UnityEngine;
using Unity.MLAgents;

public class PushAgent : Agent
{
    /// <summary>
    /// The ground. The bounds are used to spawn the elements.
    /// </summary>
    public GameObject ground;

    public GameObject area;

    [HideInInspector]
    public Bounds areaBounds;

    private VisualSettings visualSettings;

    public GameObject goal;

    public GameObject block;

    [HideInInspector]
    public GoalDetection goalDetect;

    public bool useVectorObs;

    Rigidbody blockRigid;  
    Rigidbody agentRigid; 
    Material groundMaterial; 

    Renderer groundRenderer;

    EnvironmentParameters defaultParameters;

    void Awake()
    {
        visualSettings = FindObjectOfType<VisualSettings>();
    }

    public override void Initialize()
    {
        goalDetect = block.GetComponent<GoalDetection>();
        goalDetect.agent = this;

        agentRigid = GetComponent<Rigidbody>();
        blockRigid = block.GetComponent<Rigidbody>();
        areaBounds = ground.GetComponent<Collider>().bounds;
        groundRenderer = ground.GetComponent<Renderer>();
        groundMaterial = groundRenderer.material;

        defaultParameters = Academy.Instance.EnvironmentParameters;

        SetResetParameters();
    }

    public Vector3 GetRandomSpawnPos()
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            var randomPosX = Random.Range(-areaBounds.extents.x * visualSettings.spawnAreaMarginMultiplier,
                areaBounds.extents.x * visualSettings.spawnAreaMarginMultiplier);

            var randomPosZ = Random.Range(-areaBounds.extents.z * visualSettings.spawnAreaMarginMultiplier,
                areaBounds.extents.z * visualSettings.spawnAreaMarginMultiplier);
            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, 1f, randomPosZ);
            if (Physics.CheckBox(randomSpawnPos, new Vector3(2.5f, 0.01f, 2.5f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    public void ScoredAGoal()
    {
        AddReward(5f);

        EndEpisode();

        StartCoroutine(GoalScoredSwapGroundMaterial(visualSettings.goalScoredMaterial, 0.5f));
    }

    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        groundRenderer.material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        groundRenderer.material = groundMaterial;
    }

    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = Mathf.FloorToInt(act[0]);

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        agentRigid.AddForce(dirToGo * visualSettings.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        MoveAgent(vectorAction);

        AddReward(-1f / MaxStep);
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;
        if (Input.GetKey(KeyCode.D))
        {
            actionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            actionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2;
        }
    }

    void ResetBlock()
    {
        block.transform.position = GetRandomSpawnPos();

        blockRigid.velocity = Vector3.zero;

        blockRigid.angularVelocity = Vector3.zero;
    }

    public override void OnEpisodeBegin()
    {
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        area.transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        ResetBlock();
        transform.position = GetRandomSpawnPos();
        agentRigid.velocity = Vector3.zero;
        agentRigid.angularVelocity = Vector3.zero;

        SetResetParameters();
    }

    public void SetGroundMaterialFriction()
    {
        var groundCollider = ground.GetComponent<Collider>();

        groundCollider.material.dynamicFriction = defaultParameters.GetWithDefault("dynamic_friction", 1);
        groundCollider.material.staticFriction = defaultParameters.GetWithDefault("static_friction", 0);
    }

    public void SetBlockProperties()
    {
        var scale = defaultParameters.GetWithDefault("block_scale", 2);
        blockRigid.transform.localScale = new Vector3(scale, 0.75f, scale);
        blockRigid.drag = defaultParameters.GetWithDefault("block_drag", 0.5f);
    }

    void SetResetParameters()
    {
        SetGroundMaterialFriction();
        SetBlockProperties();
    }
}

