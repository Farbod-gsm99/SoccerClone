using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class SoccerEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public AgentSoccer Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;

    private SoccerSettings m_SoccerSettings;
    public GameObject ball;
    [HideInInspector]
    public Rigidbody ballRb;
    Vector3 m_BallStartingPos;
    private int m_ResetTimer;
    public int MaxEnvironmentSteps = 25000;

    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    void Start()
    {
        m_SoccerSettings = FindFirstObjectByType<SoccerSettings>();
        m_BlueAgentGroup = new SimpleMultiAgentGroup();
        m_PurpleAgentGroup = new SimpleMultiAgentGroup();
        ballRb = ball.GetComponent<Rigidbody>();
        m_BallStartingPos = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);

        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            if (item.Agent.team == Team.Blue)
            {
                m_BlueAgentGroup.RegisterAgent(item.Agent);
            }
            else
            {
                m_PurpleAgentGroup.RegisterAgent(item.Agent);
            }
        }
        ResetScene();
    }


    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer > MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_BlueAgentGroup.GroupEpisodeInterrupted();
            m_PurpleAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }


    public void ResetScene()
    {
        m_ResetTimer = 0;

        foreach (var item in AgentsList)
        {
            var randomPosX = Random.Range(-5f, 5f);
            var newStartPos = item.Agent.initialPos + new Vector3(randomPosX, 0f, 0f);
            var rot = item.Agent.rotSign * Random.Range(80.0f, 100.0f);
            var newRot = Quaternion.Euler(0, rot, 0);
            item.Agent.transform.SetPositionAndRotation(newStartPos, newRot);

            item.Rb.linearVelocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        ResetBall();
    }

    public void ResetBall()
    {
        var RandomPosX = Random.Range(-2.5f, 2.5f);
        var RandomPosZ = Random.Range(-2.5f, 2.5f);
        ball.transform.position = m_BallStartingPos + new Vector3(RandomPosX, 0f, RandomPosZ);

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
    }


    public void GoalTouched(Team scoredTeam)
    {
        if (scoredTeam == Team.Blue)
        {
            m_BlueAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_PurpleAgentGroup.AddGroupReward(-1);
        }
        else
        {
            m_PurpleAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_BlueAgentGroup.AddGroupReward(-1);
        }
        m_PurpleAgentGroup.GroupEpisodeInterrupted();
        m_BlueAgentGroup.GroupEpisodeInterrupted();
        ResetScene();
    }
}
