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
    FiringDelay,
    Firing,
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
    private bool mouthOpen = false;

    public EFirePiranhaState State
    {
        get { return state; }
    }

    public GameObject fireballPrefab;
    
    private Vector2 shootingDirection;
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
            SetLookDirection(marioLocation, mouthOpen = false);
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
            SetLookDirection(marioLocation, mouthOpen = false);

            holdTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;

            if (holdTimer <= 0.0f)
            {
                holdTimer = 0.0f;
                SetState(EFirePiranhaState.FiringDelay);
            }
        }
        else if (state == EFirePiranhaState.AnimatingDown)
        {
            Vector2 marioLocation = Game.Instance.MarioGameObject.transform.position;
            SetLookDirection(marioLocation, mouthOpen = false);
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
        else if (state == EFirePiranhaState.FiringDelay)
        {
            //Make piranha look in the direction of mario
            Vector2 marioLocation = Game.Instance.MarioGameObject.transform.position;
            SetLookDirection(marioLocation, mouthOpen = true);
            holdTimer -= Time.deltaTime * Game.Instance.LocalTimeScale;
            if (holdTimer <= 0.0f)
            {
                SetState(EFirePiranhaState.Firing);
            }
        }
        else if (state == EFirePiranhaState.Firing)
        {
            //Shoot a fireball
            ShootFireball();
            SetState(EFirePiranhaState.AnimatingDown);
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
            else if (state == EFirePiranhaState.FiringDelay)
            {
                holdTimer = 0.5f;
            }
        }
    }

    private void SetLookDirection(Vector2 MarioLocation, bool mouthOpen)
    {
        Vector3 scale = transform.localScale;
        if (MarioLocation.y < activeLocation.y + 2.0f)
        {
            //Look Down
            if (!mouthOpen)
            {
                animator.Play("FirePiranhaDownClosed");
            }
            else
            {
                animator.Play("FirePiranhaDownOpen");
            }
            
            if (MarioLocation.x < activeLocation.x) { scale.x = 1.0f; shootingDirection = new Vector2(-1, -1); } else { scale.x = -1.0f; shootingDirection = new Vector2(1, -1); }
            transform.localScale = scale;
        }
        else
        {
            //Look Up
            if (!mouthOpen)
            {
                animator.Play("FirePiranhaUpClosed");
            }
            else 
            {
                animator.Play("FirePiranhaUpOpen");
            }

            if (MarioLocation.x < activeLocation.x) { scale.x = 1.0f; shootingDirection = new Vector2(-1, 1); } else { scale.x = -1.0f; shootingDirection = new Vector2(1, 1); }
            transform.localScale = scale;
        }
    }

    private void ShootFireball()
    {
        Vector2 marioLocation = Game.Instance.MarioGameObject.transform.position;
        Vector2 shootingPosition = transform.position + new Vector3(0.0f, EnemyConstants.FirePiranhaOffsetY -0.6f, 0.0f);
        GameObject fireball = Instantiate(fireballPrefab, shootingPosition, Quaternion.identity);
        fireball.GetComponent<FireBall>().Initialize(shootingDirection);
        Debug.Log("Fireball shot");
    }
}
