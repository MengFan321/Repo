using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class MultiGroupSwitcherGizmos : MonoBehaviour
{
    public MultiGroupSwitcher switcher;
    public Color gizmoColor = Color.red;
    public float gizmoRadius = 0.2f;

    private void OnDrawGizmos()
    {
        if (switcher == null || switcher.groups == null)
            return;

        Gizmos.color = gizmoColor;

        foreach (var group in switcher.groups)
        {
            if (group.objects == null || group.alertSettings == null)
                continue;

            for (int i = 0; i < group.objects.Count; i++)
            {
                var obj = group.objects[i];
                if (obj == null || i >= group.alertSettings.Count)
                    continue;

                var offset = group.alertSettings[i].offset;
                Vector3 pos = obj.transform.position + offset;
                Gizmos.DrawSphere(pos, gizmoRadius);

#if UNITY_EDITOR
                Handles.Label(pos + Vector3.up * 0.2f, $"¸ÐÌ¾ºÅ: {group.name} - ½×¶Î{i}");
#endif
            }
        }
    }
}
