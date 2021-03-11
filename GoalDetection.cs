using UnityEngine;

public class GoalDetection : MonoBehaviour
{
    [HideInInspector]
    public PushAgent agent;

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("goal"))
        {
            agent.ScoredAGoal();
        }
    }

}