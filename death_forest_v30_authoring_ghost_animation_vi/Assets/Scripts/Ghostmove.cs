using UnityEngine;

public class GhostMove : MonoBehaviour
{
    public float speed = 2.5f;
    public float rotateSpeed = 8f;
    public float stopDistance = 1.2f;
    public Transform target;

    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void Update()
    {
        if (controller == null)
        {
            Debug.LogError("GhostMove: thiếu CharacterController trên object " + gameObject.name);
            return;
        }

        if (!controller.enabled || !gameObject.activeInHierarchy)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        float distance = dir.magnitude;

        if (distance <= stopDistance)
        {
            return;
        }

        dir.Normalize();

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRot,
                rotateSpeed * Time.deltaTime
            );
        }

        controller.Move(dir * speed * Time.deltaTime);
    }
}