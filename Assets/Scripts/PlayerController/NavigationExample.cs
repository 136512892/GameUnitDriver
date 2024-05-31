using UnityEngine;
using UnityEngine.AI;

public class NavigationExample : MonoBehaviour
{
    [SerializeField] private NavMeshAgent[] agents;
    [SerializeField] private Transform destination;
    [SerializeField] private OffMeshLink[] links;

    private void Update()
    {
        for (int i = 0; i < agents.Length; i++)
        {
            NavMeshAgent agent = agents[i];
            //到达目的地
            if (Vector3.Distance(agent.transform.position,
                agent.destination) <= agent.stoppingDistance)
                agent.GetComponent<Animator>().SetBool("Move", false);
            else //未到达目的地
                agent.GetComponent<Animator>().SetBool("Move", true);
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("设置寻路目的地", 
            GUILayout.Width(200f), GUILayout.Height(50f)))
        {
            for (int i = 0; i < agents.Length; i++)
                agents[i].SetDestination(destination.position);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(destination.position, .3f);
        Gizmos.color = Color.white;
        for (int i = 0;i < agents.Length; i++)
        {
            Vector3[] path = agents[i].path.corners;
            for (int j = 0; j < path.Length - 1; j++)
            {
                Gizmos.DrawLine(path[j], path[j + 1]);
            }
        }

#if UNITY_EDITOR
        if (links != null && links.Length > 0)
        {
            for (int i = 0; i < links.Length; i++)
            {
                OffMeshLink link = links[i];
                if (link.startTransform != null && link.endTransform != null)
                {
                    GUIStyle style = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 30,
                        fontStyle = FontStyle.Bold
                    };
                    UnityEditor.Handles.Label(link.startTransform.position, "A", style);
                    UnityEditor.Handles.Label(link.endTransform.position, "B", style);
                }
            }
        }
#endif
    }
}