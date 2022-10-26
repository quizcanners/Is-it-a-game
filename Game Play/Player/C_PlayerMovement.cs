using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class C_PlayerMovement : MonoBehaviour
{
    [SerializeField] private LayerMask LayerMask;
    [SerializeField] private AudioSource AudioSource;
    [SerializeField] private List<AudioClip> Clips;
    [SerializeField] private NavMeshAgent Agent;
    [SerializeField] private Animator Animator;

    private readonly AnimatorValue.Float _speed = new("Speed");

    private Vector2 Velocity;
    private Vector2 SmoothDeltaPosition;

    private Camera Camera => Singleton.Get<Singleton_CameraOperatorGodMode>().MainCam;

    private void Awake()
    {
        Animator.applyRootMotion = true;
        Agent.updatePosition = false;
        Agent.updateRotation = true;
    }

    private void OnAnimatorMove()
    {
        Vector3 rootPosition = Animator.rootPosition;
        rootPosition.y = Agent.nextPosition.y;
        transform.position = rootPosition;
        Agent.nextPosition = rootPosition;
    }

    private void Update()
    {
        HandleInput();
        SynchronizeAnimatorAndAgent();
    }

    private void SynchronizeAnimatorAndAgent()
    {
        Assert.AreNotApproximatelyEqual(Agent.stoppingDistance, 0);

        Vector3 worldDeltaPosition = Agent.nextPosition - transform.position;
        worldDeltaPosition.y = 0;

        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        float smooth = Mathf.Min(1, Time.deltaTime / 0.1f);
        SmoothDeltaPosition = Vector2.Lerp(SmoothDeltaPosition, deltaPosition, smooth);

        Velocity = SmoothDeltaPosition / Time.deltaTime;
        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            Velocity = Vector2.Lerp(
                Vector2.zero,
                Velocity,
                Agent.remainingDistance / Agent.stoppingDistance
            );
        }

      //  bool shouldMove = Velocity.magnitude > 0.5f
         //   && Agent.remainingDistance > Agent.stoppingDistance;

        _speed.SetOn(Velocity.magnitude, Animator);

        //Animator.SetBool("move", shouldMove);
        //Animator.SetFloat("locomotion", Velocity.magnitude);

        float deltaMagnitude = worldDeltaPosition.magnitude;
        if (deltaMagnitude > Agent.radius / 2f)
        {
            transform.position = Vector3.Lerp(
                Animator.rootPosition,
                Agent.nextPosition,
                smooth
            );
        }
    }

    private void HandleInput()
    {
        if (Application.isFocused && Mouse.current.rightButton.isPressed)
        {
            Ray ray = Camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask))
            {
                Agent.SetDestination(hit.point);
            }
        }
    }

    public void PlayAudio()
    {
        AudioSource.PlayOneShot(Clips[Random.Range(0, Clips.Count)]);
    }
}