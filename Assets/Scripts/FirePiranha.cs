using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum EFirePiranhaState : byte
{
    Unknown,
    Hiding,
    AnimatingUp,
    Active,
    AnimatingDown
}

public class FirePiranha : Enemy
{
    private Animator animator;

    private EFirePiranhaState state = EFirePiranhaState.Unknown;
    private Vector2 hidingLocation = Vector2.zero;
    private Vector2 activeLocation = Vector2.zero;
    private float holdTimer = 0.0f;
    private float animationTimer = 0.0f;

    public EFirePiranhaState State
    {
        get { return state; }
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        // Set the enemy type
        enemyType = EEnemyType.FirePiranha;

        // Capture the starting location (hiding) and from that calculate the active (on-screen) location
        hidingLocation = transform.position;
        activeLocation = hidingLocation + new Vector2(0.0f, EnemyConstants.FirePiranhaOffsetY);

        // Set the state to hiding
        SetState(EFirePiranhaState.Hiding);
    }

    // Update is called once per frame
    void Update()
    {
        if (state == EFirePiranhaState.Hiding)
        {
            holdTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            if (holdTimer <= 0.0f)
            {
                holdTimer = 0.0f;
                SetState(EFirePiranhaState.AnimatingUp);
            }
        }
        else if (state == EFirePiranhaState.AnimatingUp)
        {
            Vector2 marioLocation = Game.Instance.MarioGameObject.transform.position;
            SetLookDirection(marioLocation);
            animationTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            float pct = 1.0f - (animationTimer / EnemyConstants.PiranhaPlantAnimationDuration);
            float locationX = Mathf.Lerp(hidingLocation.x, activeLocation.x, pct);
            float locationY = Mathf.Lerp(hidingLocation.y, activeLocation.y, pct);
            transform.position = new Vector2(locationX, locationY);

            if (animationTimer <= 0.0f)
            {
                animationTimer = 0.0f;
                SetState(EFirePiranhaState.Active);
            }
        }
        else if (state == EFirePiranhaState.Active)
        {
            //Make piranha look in the direction of mario
            Vector2 marioLocation = Game.Instance.MarioGameObject.transform.position;
            SetLookDirection(marioLocation);

            holdTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            if (holdTimer <= 0.0f)
            {
                holdTimer = 0.0f;
                SetState(EFirePiranhaState.AnimatingDown);
            }
        }
        else if (state == EFirePiranhaState.AnimatingDown)
        {
            Vector2 marioLocation = Game.Instance.MarioGameObject.transform.position;
            SetLookDirection(marioLocation);
            animationTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            float pct = 1.0f - (animationTimer / EnemyConstants.PiranhaPlantAnimationDuration);
            float locationX = Mathf.Lerp(activeLocation.x, hidingLocation.x, pct);
            float locationY = Mathf.Lerp(activeLocation.y, hidingLocation.y, pct);
            transform.position = new Vector2(locationX, locationY);

            if (animationTimer <= 0.0f)
            {
                animationTimer = 0.0f;
                SetState(EFirePiranhaState.Hiding);
            }
        }
    }

    private void SetState(EFirePiranhaState newState)
    {
        if (state != newState)
        {
            state = newState;

            if (state == EFirePiranhaState.Hiding)
            {
                transform.position = hidingLocation;
                holdTimer = UnityEngine.Random.Range(EnemyConstants.PiranhaPlantHiddenDurationMin, EnemyConstants.PiranhaPlantHiddenDurationMax);
            }
            else if (state == EFirePiranhaState.AnimatingUp)
            {
                // Get Mario's location
                Vector2 marioLocation = Game.Instance.MarioGameObject.transform.position;

                // Check if Mario is on top of the pipe, if he is, don't spawn the piranha plant
                bool checkY = Mathf.Clamp(marioLocation.x, activeLocation.x - 1.0f, activeLocation.x + 1.0f) == marioLocation.x;
                if (checkY && Mathf.Abs(activeLocation.y - marioLocation.y) <= 0.51f)
                {
                    SetState(EFirePiranhaState.Hiding);
                    return;
                }

                animationTimer = EnemyConstants.PiranhaPlantAnimationDuration;
            }
            else if (state == EFirePiranhaState.Active)
            {
                transform.position = activeLocation;
                holdTimer = EnemyConstants.PiranhaPlantActiveDuration;
            }
            else if (state == EFirePiranhaState.AnimatingDown)
            {
                animationTimer = EnemyConstants.PiranhaPlantAnimationDuration;
            }
        }
    }

    private void SetLookDirection(Vector2 MarioLocation)
    {
        if (MarioLocation.x < activeLocation.x && MarioLocation.y < activeLocation.y + 2.0f)
        {
            //Look BottomLeft
            animator.Play("FirePiranhaDownClosed");
            Vector3 scale = transform.localScale;
            scale.x = 1.0f;
            transform.localScale = scale;
        }
        else if (MarioLocation.x > activeLocation.x && MarioLocation.y < activeLocation.y + 2.0f)
        {
            //Look BottomRight
            animator.Play("FirePiranhaDownClosed");
            Vector3 scale = transform.localScale;
            scale.x = -1.0f;
            transform.localScale = scale;
        }
        else if (MarioLocation.x > activeLocation.x && MarioLocation.y > activeLocation.y + 2.0f)
        {
            //Look TopRight
            animator.Play("FirePiranhaUpClosed");
        }
        else if (MarioLocation.x < activeLocation.x && MarioLocation.y > activeLocation.y + 2.0f)
        {
            //Look TopLeft
            animator.Play("FirePiranhaUpClosed");
        }
    }
}
