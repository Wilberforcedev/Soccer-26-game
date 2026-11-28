using System.Collections.Generic;
using UnityEngine;
using FC2026.Core;
using FC2026.World;

namespace FC2026.Gameplay
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class FootballPlayerController : MonoBehaviour
    {
        private static readonly List<FootballPlayerController> Registry = new();
        private static FootballPlayerController previousSwitchSource;
        private static float previousSwitchAt = -99f;
        private const float SwitchCycleWindow = 1.1f;
        private const float MaxTackleBallHeight = 1.55f;
        private const float ShotChargeDuration = 0.9f;
        private const float LoftPassHoldDuration = 0.28f;
        private const float HeaderContactMinHeight = 0.95f;
        private const float HeaderContactMaxHeight = 2.35f;

        [SerializeField] private float moveSpeed = 7.1f;
        [SerializeField] private float sprintSpeed = 9.7f;
        [SerializeField] private float acceleration = 21f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private float controlRadius = 0.95f;
        [SerializeField] private float kickReach = 2.15f;
        [SerializeField] private float staminaDrain = 0.09f;
        [SerializeField] private float staminaRecovery = 0.05f;

        private Rigidbody body;
        private FootballMotionAnimator motion;
        private bool passQueued;
        private bool loftedPassQueued;
        private bool dinkedPassQueued;
        private bool dinkInputHeld;
        private bool mobileDinkHeld;
        private float dinkHoldTime;
        private float queuedDinkPower01;
        private bool passInputHeld;
        private bool mobilePassHeld;
        private float passHoldTime;
        private bool throughBallQueued;
        private bool crossQueued;
        private bool shootQueued;
        private bool chipShotQueued;
        private bool finesseShotQueued;
        private bool lowDrivenShotQueued;
        private bool tackleQueued;
        private bool mobileSecondManPressHeld;
        private bool mobileShieldHeld;
        private bool mobileCornerShieldHeld;
        private bool mobileCornerAvoidanceHeld;
        private bool passFollowArmed;
        private bool passFollowRunActive;
        private Vector3 passFollowStart;
        private Vector3 passFollowTarget;
        private float passFollowSide;
        private float passFollowTime;
        private bool cornerAvoidanceActive;
        private readonly List<(Collider own, Collider other)> cornerIgnoredCollisions = new();

        private bool mobileShotHeld;
        private bool shotChargeInputHeld;
        private float shotChargeTime;
        private float queuedShotPower01;
        private Vector3 lastMoveDirection = Vector3.forward;
        private Vector2 mobileMove;
        private bool mobileSprint;
        private float stamina = 1f;
        private float offBalanceTimer;
        private float offBalanceSpeedFactor = 1f;
        private float offBalanceAccelerationFactor = 1f;
        private float kickLock;
        private float tackleCooldown;
        private float shootingStat = 0.6f;
        private float passingStat = 0.6f;
        private float defendingStat = 0.6f;
        private float physicalStat = 0.5f;
        private float paceMultiplier = 1f;
        private bool prefersLeftFoot;
        private int skinTone = -1;
        private int hairColor = -1;
        private int faceVariant = -1;

        public bool IsUserControlled { get; set; } = true;
        public int TeamDirection { get; set; } = 1;
        public int TeamId { get; private set; } = 1;
        public PlayerRole Role { get; private set; } = PlayerRole.Midfielder;
        public Vector3 HomePosition { get; private set; }
        public float Stamina01 => stamina;
        public float ShootingStat => shootingStat;
        public float PassingStat => passingStat;
        public float DefendingStat => defendingStat;
        public float PhysicalStat => physicalStat;
        public float ShotCharge01 => Mathf.Clamp01(shotChargeTime / ShotChargeDuration);
        public float PassHold01 => Mathf.Clamp01(passHoldTime / LoftPassHoldDuration);
        public float DinkHold01 => Mathf.Clamp01(dinkHoldTime / 0.65f);
        public float ContactSpeedFactor => offBalanceTimer > 0f ? offBalanceSpeedFactor : 1f;
        public float ContactAccelerationFactor => offBalanceTimer > 0f ? offBalanceAccelerationFactor : 1f;
        public bool PassFollowArmed => passFollowArmed;
        public bool IsShielding => IsUserControlled && HasBall &&
                                   (mobileShieldHeld || Input.GetKey(KeyCode.V));
        public bool IsGoalkeeper => Role == PlayerRole.Goalkeeper;
        public bool HasBall => BallController.Instance != null && BallController.Instance.IsOwnedBy(this);
        public Vector3 LastMoveDirection => lastMoveDirection;
        public Rigidbody Body => body;
        public static FootballPlayerController ActiveUserPlayer { get; private set; }
        public static FootballPlayerController SecondaryPressPlayer { get; private set; }

        public static IReadOnlyList<FootballPlayerController> All => Registry;

        public static FootballPlayerController FindNearestTo(Vector3 position, int teamDirection, bool excludeGoalkeeper = false)
        {
            FootballPlayerController nearest = null;
            var best = float.MaxValue;
            foreach (var candidate in Registry)
            {
                if (candidate == null || candidate.TeamDirection != teamDirection)
                    continue;
                if (excludeGoalkeeper && candidate.IsGoalkeeper)
                    continue;
                var delta = candidate.transform.position - position;
                delta.y = 0f;
                if (delta.sqrMagnitude < best)
                {
                    best = delta.sqrMagnitude;
                    nearest = candidate;
                }
            }
            return nearest;
        }

        public void Configure(PlayerRole role, Vector3 homePosition, int teamDirection, PlayerMatchProfile profile)
        {
            Role = role;
            HomePosition = homePosition;
            TeamDirection = teamDirection >= 0 ? 1 : -1;
            TeamId = TeamDirection;
            shootingStat = Mathf.Clamp01(profile.Shooting);
            passingStat = Mathf.Clamp01(profile.Passing);
            defendingStat = Mathf.Clamp01(profile.Defending);
            physicalStat = Mathf.Clamp01(profile.Physical);
            paceMultiplier = Mathf.Clamp(profile.Pace, 0.88f, 1.14f);
            prefersLeftFoot = profile.LeftFooted;
            skinTone = profile.SkinTone;
            hairColor = profile.HairColor;
            faceVariant = profile.FaceVariant;
            lastMoveDirection = new Vector3(0f, 0f, TeamDirection);
        }

        public void FlipSides()
        {
            TeamDirection = -TeamDirection;
            HomePosition = new Vector3(-HomePosition.x, HomePosition.y, -HomePosition.z);
            lastMoveDirection = new Vector3(0f, 0f, TeamDirection);
        }

        public void SetUserControlled(bool active)
        {
            if (!active)
            {
                SetCornerAvoidanceHeld(false);
                mobileCornerShieldHeld = false;
                mobileCornerAvoidanceHeld = false;
                passFollowArmed = false;
                passFollowRunActive = false;
            }

            IsUserControlled = active;
            if (active)
            {
                ActiveUserPlayer = this;
                ClearQueuedActions();
            }
            else if (ActiveUserPlayer == this)
            {
                ActiveUserPlayer = null;
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void OnEnable()
        {
            if (!Registry.Contains(this))
                Registry.Add(this);
        }

        private void OnDisable()
        {
            RestoreCornerAvoidance();
            Registry.Remove(this);
            if (ActiveUserPlayer == this)
                ActiveUserPlayer = null;
            if (SecondaryPressPlayer == this)
                SecondaryPressPlayer = null;
        }

        private void Start()
        {
            var styleSeed = (WorldHash.Combine(gameObject.name) & 0xFFFFFFu) / 16777216f;
            motion = FootballVisualFactory.EnsureAnimatedRig(gameObject, IsGoalkeeper, styleSeed,
                false, skinTone, hairColor, faceVariant);
        }

        private void Update()
        {
            if (!IsUserControlled || (!MatchManager3D.IsLive && !MatchManager3D.IsCornerPrep))
                return;

            if (Application.isMobilePlatform && Input.touchCount == 0)
            {
                mobileMove = Vector2.zero;
                mobileSprint = false;
                mobilePassHeld = false;
                mobileDinkHeld = false;
                mobileSecondManPressHeld = false;
                mobileShieldHeld = false;
                mobileShotHeld = false;
                mobileCornerShieldHeld = false;
                mobileCornerAvoidanceHeld = false;
            }

            if (MatchManager3D.IsCornerPrep)
            {
                SetCornerAvoidanceHeld(Input.GetKey(KeyCode.Z) || mobileCornerAvoidanceHeld);
                return;
            }

            SetCornerAvoidanceHeld(false);
            mobileCornerShieldHeld = false;
            mobileCornerAvoidanceHeld = false;

            var passHeld = mobilePassHeld || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton2);
            if (passHeld)
            {
                passHoldTime = Mathf.Min(LoftPassHoldDuration, passHoldTime + Time.deltaTime);
            }
            else if (passInputHeld)
            {
                if (passHoldTime >= LoftPassHoldDuration * 0.85f)
                    QueueLoftedPass();
                else
                    QueuePass();
                passHoldTime = 0f;
            }
            passInputHeld = passHeld;

            var dinkHeld = mobileDinkHeld || Input.GetKey(KeyCode.T);
            if (dinkHeld)
            {
                dinkHoldTime = Mathf.Min(0.65f, dinkHoldTime + Time.deltaTime);
            }
            else if (dinkInputHeld)
            {
                queuedDinkPower01 = Mathf.Clamp01(dinkHoldTime / 0.65f);
                QueueDinkedPass();
                dinkHoldTime = 0f;
            }
            dinkInputHeld = dinkHeld;
            if (passFollowArmed && !HasBall && !passInputHeld)
                passFollowArmed = false;

            UpdateSecondaryPress();
            var shotHeld = mobileShotHeld || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton0);
            if (shotHeld)
            {
                shotChargeTime = Mathf.Min(ShotChargeDuration, shotChargeTime + Time.deltaTime);
            }
            else if (shotChargeInputHeld)
            {
                queuedShotPower01 = ShotCharge01;
                shotChargeTime = 0f;
                shootQueued = true;
            }
            shotChargeInputHeld = shotHeld;

            if (Input.GetKeyDown(KeyCode.R))
                QueueLoftedPass();
            if (Input.GetKeyDown(KeyCode.Q))
                throughBallQueued = true;
            if (Input.GetKeyDown(KeyCode.B))
                QueueLowDrivenShot();
            if (Input.GetKeyDown(KeyCode.F))
                QueueCross();
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (HasBall && (Input.GetKey(KeyCode.E) || passInputHeld))
                    passFollowArmed = true;
                else
                    SimpleFootballAI.TriggerCurvedRun(this);
            }
            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.JoystickButton1))
                tackleQueued = true;
            if (Input.GetKeyDown(KeyCode.Tab))
                SwitchToNearestTeammate();
        }

        private void FixedUpdate()
        {
            offBalanceTimer = Mathf.Max(0f, offBalanceTimer - Time.fixedDeltaTime);
            kickLock = Mathf.Max(0f, kickLock - Time.fixedDeltaTime);
            tackleCooldown = Mathf.Max(0f, tackleCooldown - Time.fixedDeltaTime);

            if (!IsUserControlled)
            {
                if (!MatchManager3D.IsLive)
                {
                    UpdateStamina(false, 0f);
                    return;
                }

                var aiSpeed = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z).magnitude;
                UpdateStamina(aiSpeed > moveSpeed * 1.02f, aiSpeed);
                return;
            }

            if (MatchManager3D.IsCornerPrep)
            {
                UpdateCornerPrepMovement();
                return;
            }

            if (!MatchManager3D.IsLive)
            {
                ClearQueuedActions();
                Decelerate(acceleration);
                UpdateStamina(false, 0f);
                return;
            }

            var move = ReadMove();
            var shielding = IsShielding;
            var desired = new Vector3(move.x, 0f, move.y);
            if (desired.sqrMagnitude > 1f)
                desired.Normalize();

            var passFollowRunning = false;
            if (passFollowRunActive)
            {
                if (move.sqrMagnitude > 0.08f)
                {
                    passFollowRunActive = false;
                }
                else
                {
                    var progress = 1f - Mathf.Clamp01(passFollowTime / 2.7f);
                    var curve = Vector3.right * (Mathf.Sin(progress * Mathf.PI) * passFollowSide * 2.6f);
                    var current = Flat(transform.position);
                    var followPoint = Vector3.Lerp(passFollowStart, passFollowTarget, progress) + curve;
                    var followDirection = followPoint - current;
                    var remaining = passFollowTarget - current;
                    if (remaining.sqrMagnitude < 1f || passFollowTime <= 0f)
                    {
                        passFollowRunActive = false;
                    }
                    else
                    {
                        if (followDirection.sqrMagnitude < 0.2f)
                            followDirection = remaining;
                        desired = followDirection.normalized;
                        passFollowRunning = true;
                        passFollowTime = Mathf.Max(0f, passFollowTime - Time.fixedDeltaTime);
                    }
                }
            }

            var sprinting = ReadSprint() && move.sqrMagnitude > 0.02f &&
                            stamina > 0.04f && !shielding && !passFollowRunning;
            var carryFactor = HasBall ? 0.93f : 1f;
            var fatigueFactor = Mathf.Lerp(0.74f, 1f, stamina);
            var shieldingFactor = shielding ? 0.68f : 1f;
            // The selected player's pace rating scales their movement and sprint speed.
            var desiredSpeed = (passFollowRunning
                ? moveSpeed * 0.92f * paceMultiplier * fatigueFactor
                : (sprinting ? sprintSpeed : moveSpeed) * paceMultiplier *
                  fatigueFactor * carryFactor * shieldingFactor) *
                               (offBalanceTimer > 0f ? offBalanceSpeedFactor : 1f);

            var horizontal = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
            if (desired.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = desired.normalized;
                var targetVelocity = desired * desiredSpeed;
                var contactAcceleration = offBalanceTimer > 0f
                    ? acceleration * offBalanceAccelerationFactor
                    : acceleration;
                var changed = Vector3.MoveTowards(horizontal, targetVelocity, contactAcceleration * Time.fixedDeltaTime);
                body.linearVelocity = new Vector3(changed.x, body.linearVelocity.y, changed.z);

                var look = Quaternion.LookRotation(lastMoveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.fixedDeltaTime);
            }
            else
            {
                Decelerate(acceleration * 0.85f);
            }

            UpdateStamina(sprinting, horizontal.magnitude);

            if (tackleQueued)
            {
                tackleQueued = false;
                AttemptTackle();
            }

            if (passQueued)
            {
                passQueued = false;
                var lofted = loftedPassQueued;
                var dinked = dinkedPassQueued;
                var dinkPower = queuedDinkPower01;
                loftedPassQueued = false;
                dinkedPassQueued = false;
                queuedDinkPower01 = 0f;
                AttemptPass(false, lofted, dinked, dinkPower);
            }

            if (throughBallQueued)
            {
                throughBallQueued = false;
                AttemptPass(true);
            }

            if (crossQueued)
            {
                crossQueued = false;
                AttemptCross();
            }

            if (shootQueued)
            {
                shootQueued = false;
                AttemptShot(chipShotQueued, finesseShotQueued, null, queuedShotPower01, lowDrivenShotQueued);
                queuedShotPower01 = 0f;
                chipShotQueued = false;
                finesseShotQueued = false;
                lowDrivenShotQueued = false;
            }

            TryClaimLooseBall();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!HasBall || !MatchManager3D.IsLive || collision == null || collision.contactCount == 0)
                return;

            var opponent = collision.collider.GetComponentInParent<FootballPlayerController>();
            if (opponent == null || opponent.TeamId == TeamId)
                return;

            var impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < 1.15f)
                return;

            // The match profile has physical, pace, and passing ratings rather than separate
            // balance, agility, and dribbling ratings; use them as the current control proxy.
            var dribbleControl = (
                physicalStat +
                Mathf.InverseLerp(0.88f, 1.14f, paceMultiplier) +
                passingStat) / 3f;
            var contactStrength = Mathf.InverseLerp(1.15f, 3.8f, impactSpeed);
            var recoveryTime = Mathf.Lerp(0.48f, 0.18f, dribbleControl) *
                               Mathf.Lerp(0.65f, 1f, contactStrength);

            offBalanceTimer = Mathf.Max(offBalanceTimer, recoveryTime);
            offBalanceSpeedFactor = Mathf.Lerp(0.58f, 0.9f, dribbleControl);
            offBalanceAccelerationFactor = Mathf.Lerp(0.55f, 0.9f, dribbleControl);
        }

        private void UpdateCornerPrepMovement()
        {
            var move = ReadMove();
            var shielding = mobileCornerShieldHeld || Input.GetKey(KeyCode.V);
            var sprinting = ReadSprint() && move.sqrMagnitude > 0.02f && stamina > 0.04f && !shielding;
            var desired = new Vector3(move.x, 0f, move.y);
            if (desired.sqrMagnitude > 1f)
                desired.Normalize();

            var speed = (sprinting ? sprintSpeed : moveSpeed) * paceMultiplier *
                        Mathf.Lerp(0.74f, 1f, stamina) * (shielding ? 0.68f : 1f);
            var horizontal = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
            if (desired.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = desired.normalized;
                var targetVelocity = desired * speed;
                var changed = Vector3.MoveTowards(horizontal, targetVelocity, acceleration * Time.fixedDeltaTime);
                var next = body.position + changed * Time.fixedDeltaTime;
                next.x = Mathf.Clamp(next.x, -32.5f, 32.5f);
                next.z = Mathf.Clamp(next.z, -51.5f, 51.5f);
                body.position = next;
                body.linearVelocity = new Vector3(changed.x, body.linearVelocity.y, changed.z);

                var look = Quaternion.LookRotation(lastMoveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.fixedDeltaTime);
            }
            else
            {
                Decelerate(acceleration * (shielding ? 1.5f : 0.85f));
            }

            UpdateStamina(sprinting, horizontal.magnitude);
        }

        private void Decelerate(float rate)
        {
            var horizontal = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
            var changed = Vector3.MoveTowards(horizontal, Vector3.zero, rate * Time.fixedDeltaTime);
            body.linearVelocity = new Vector3(changed.x, body.linearVelocity.y, changed.z);
        }

        private void UpdateStamina(bool sprinting, float speed)
        {
            var drainFactor = Mathf.Lerp(1.15f, 0.85f, physicalStat);
            var recoveryFactor = Mathf.Lerp(0.85f, 1.15f, physicalStat);
            if (sprinting)
                stamina -= staminaDrain * drainFactor * Time.fixedDeltaTime;
            else
                stamina += (speed < 1.2f ? staminaRecovery : staminaRecovery * 0.55f) * recoveryFactor * Time.fixedDeltaTime;
            stamina = Mathf.Clamp01(stamina);
        }

        private void TryClaimLooseBall()
        {
            var ball = BallController.Instance;
            if (CanControlLooseBall(ball))
                ball.Claim(this);
        }

        public bool CanControlLooseBall(BallController ball)
        {
            if (ball == null || !ball.IsFree || kickLock > 0f || !ball.CanBeControlledBy(this))
                return false;
            if (!IsGoalkeeper && ball.Position.y >= HeaderContactMinHeight)
                return false;

            var delta = ball.Position - transform.position;
            delta.y = 0f;
            var horizontalSpeed = new Vector2(ball.Velocity.x, ball.Velocity.z).magnitude;
            var controlRadiusAtSpeed = controlRadius * Mathf.Lerp(
                1f, 0.55f, Mathf.InverseLerp(8f, 24f, horizontalSpeed));
            return delta.sqrMagnitude <= controlRadiusAtSpeed * controlRadiusAtSpeed;
        }

        private Vector2 ReadMove()
        {
            var move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (Application.isMobilePlatform)
                move = mobileMove;
            return Vector2.ClampMagnitude(move, 1f);
        }

        private bool ReadSprint()
        {
            var keyboardSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            return keyboardSprint || (Application.isMobilePlatform && mobileSprint);
        }

        public void SetMobileMove(Vector2 direction, bool sprint = false)
        {
            mobileMove = Vector2.ClampMagnitude(direction, 1f);
            mobileSprint = sprint;
        }

        public void SetMobileShotHeld(bool held) => mobileShotHeld = held;
        public void SetMobilePassHeld(bool held) => mobilePassHeld = held;
        public void SetMobileDinkHeld(bool held) => mobileDinkHeld = held;
        public void SetMobileSecondManPressHeld(bool held) => mobileSecondManPressHeld = held;
        public void SetMobileShieldHeld(bool held) => mobileShieldHeld = held;

        public void SetMobileCornerShieldHeld(bool held) => mobileCornerShieldHeld = held;
        public void TogglePassFollowArm()
        {
            if (HasBall)
                passFollowArmed = !passFollowArmed;
        }

        public void SetMobileCornerAvoidanceHeld(bool held)
        {
            mobileCornerAvoidanceHeld = held;
            SetCornerAvoidanceHeld(held);
        }
        public void SetCornerAvoidanceHeld(bool held)
        {
            if (cornerAvoidanceActive == held)
                return;

            cornerAvoidanceActive = held;
            if (held)
            {
                var ownCollider = GetComponent<Collider>();
                if (ownCollider == null)
                    return;

                foreach (var candidate in Registry)
                {
                    if (candidate == null || candidate.TeamDirection == TeamDirection)
                        continue;

                    var otherCollider = candidate.GetComponent<Collider>();
                    if (otherCollider == null || !ownCollider.enabled || !otherCollider.enabled)
                        continue;

                    Physics.IgnoreCollision(ownCollider, otherCollider, true);
                    cornerIgnoredCollisions.Add((ownCollider, otherCollider));
                }
                return;
            }

            RestoreCornerAvoidance();
        }

        private void RestoreCornerAvoidance()
        {
            foreach (var pair in cornerIgnoredCollisions)
            {
                if (pair.own != null && pair.other != null)
                    Physics.IgnoreCollision(pair.own, pair.other, false);
            }
            cornerIgnoredCollisions.Clear();
            cornerAvoidanceActive = false;
        }

        private void UpdateSecondaryPress()
        {
            SecondaryPressPlayer = null;
            var ball = BallController.Instance;
            var pressHeld = mobileSecondManPressHeld || Input.GetKey(KeyCode.C);
            if (!pressHeld || ball == null || ball.Owner == null || ball.Owner.TeamId == TeamId)
                return;

            var ballCarrier = ball.Owner;
            var bestDistance = 28f * 28f;
            foreach (var candidate in Registry)
            {
                if (candidate == null || candidate == this || candidate.IsUserControlled ||
                    candidate.IsGoalkeeper || candidate.TeamId != TeamId)
                    continue;

                var separation = candidate.transform.position - ballCarrier.transform.position;
                separation.y = 0f;
                if (separation.sqrMagnitude >= bestDistance)
                    continue;

                bestDistance = separation.sqrMagnitude;
                SecondaryPressPlayer = candidate;
            }
        }

        public void QueuePass() { loftedPassQueued = false; dinkedPassQueued = false; queuedDinkPower01 = 0f; passQueued = true; }
        public void QueueLoftedPass() { loftedPassQueued = true; dinkedPassQueued = false; queuedDinkPower01 = 0f; passQueued = true; }
        public void QueueDinkedPass() { loftedPassQueued = false; dinkedPassQueued = true; passQueued = true; }
        public void QueueThroughBall() => throughBallQueued = true;
        public void QueueCross() => crossQueued = true;
        public void QueueShot() { queuedShotPower01 = 0f; lowDrivenShotQueued = false; shootQueued = true; }
        public void QueueChipShot() { queuedShotPower01 = 0f; lowDrivenShotQueued = false; chipShotQueued = true; shootQueued = true; }
        public void QueueFinesseShot() { queuedShotPower01 = 0f; lowDrivenShotQueued = false; finesseShotQueued = true; shootQueued = true; }
        public void QueueLowDrivenShot()
        {
            queuedShotPower01 = 0f;
            chipShotQueued = false;
            finesseShotQueued = false;
            lowDrivenShotQueued = true;
            shootQueued = true;
        }
        public void QueueTackle() => tackleQueued = true;

        public void ClearQueuedActions()
        {
            passQueued = false;
            loftedPassQueued = false;
            dinkedPassQueued = false;
            dinkInputHeld = false;
            mobileDinkHeld = false;
            dinkHoldTime = 0f;
            queuedDinkPower01 = 0f;
            passInputHeld = false;
            mobilePassHeld = false;
            passHoldTime = 0f;
            throughBallQueued = false;
            crossQueued = false;
            shootQueued = false;
            chipShotQueued = false;
            finesseShotQueued = false;
            lowDrivenShotQueued = false;
            tackleQueued = false;
            mobileShotHeld = false;
            mobileSecondManPressHeld = false;
            mobileShieldHeld = false;
            passFollowArmed = false;
            passFollowRunActive = false;
            passFollowTime = 0f;
            SecondaryPressPlayer = null;
            shotChargeInputHeld = false;
            shotChargeTime = 0f;
            queuedShotPower01 = 0f;
        }

        public FootballPlayerController SwitchToNearestTeammate()
        {
            var ball = BallController.Instance;
            var reference = ball != null ? ball.Position : transform.position;
            var cyclePrevious = Time.time - previousSwitchAt <= SwitchCycleWindow
                ? previousSwitchSource
                : null;
            FootballPlayerController nearest = null;
            FootballPlayerController previousFallback = null;
            var nearestDistance = float.MaxValue;
            var fallbackDistance = float.MaxValue;

            foreach (var candidate in Registry)
            {
                if (candidate == null || candidate == this || candidate.TeamDirection != TeamDirection ||
                    candidate.IsUserControlled || candidate.IsGoalkeeper)
                    continue;

                var toBall = candidate.transform.position - reference;
                toBall.y = 0f;
                var distance = toBall.sqrMagnitude;

                // Consecutive switch taps cycle past the player we just left,
                // instead of snapping straight back to that same player.
                if (candidate == cyclePrevious)
                {
                    if (distance < fallbackDistance)
                    {
                        fallbackDistance = distance;
                        previousFallback = candidate;
                    }
                    continue;
                }

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }

            // Keep switching functional in small squads with no alternate outfielder.
            if (nearest == null)
                nearest = previousFallback;
            if (nearest == null)
                return this;

            previousSwitchSource = this;
            previousSwitchAt = Time.time;

            var oldAi = GetComponent<SimpleFootballAI>();
            if (oldAi != null) oldAi.enabled = true;
            SetUserControlled(false);

            var newAi = nearest.GetComponent<SimpleFootballAI>();
            if (newAi != null) newAi.enabled = false;
            nearest.SetUserControlled(true);
            return nearest;
        }

        public bool AttemptTackle()
        {
            if (tackleCooldown > 0f)
                return false;

            tackleCooldown = 0.58f;
            motion?.TriggerTackle();
            var direction = lastMoveDirection.sqrMagnitude > 0.01f ? lastMoveDirection.normalized : transform.forward;
            body.AddForce(direction * 2.1f, ForceMode.VelocityChange);

            var ball = BallController.Instance;
            if (ball == null)
                return false;

            var delta = ball.Position - transform.position;
            delta.y = 0f;
            // Tackles stay grounded so players cannot win possession from a high ball.
            if (delta.magnitude > 2.05f || ball.Position.y > MaxTackleBallHeight || ball.Owner == this)
                return false;

            if (ball.Owner == null)
            {
                if (!ball.IsFree)
                    return false;
                ball.Claim(this);
                MatchManager3D.Instance?.RecordTackleWon(TeamId);
                return true;
            }

            if (ball.Owner.TeamDirection == TeamDirection)
                return false;

            var facingBall = Vector3.Dot(transform.forward, delta.normalized);
            var approachBonus = Mathf.Lerp(-0.12f, 0.06f, Mathf.Clamp01((facingBall + 1f) * 0.5f));
            var defenderBonus = Role == PlayerRole.Defender ? 0.12f : Role == PlayerRole.Midfielder ? 0.05f : 0f;
            var skillBonus = (defendingStat - 0.6f) * 0.24f;
            // Stronger carriers resist contact; facing the ball improves the tackler's approach.
            var carrierResistance = (ball.Owner.PhysicalStat - 0.5f) * 0.18f;
            var shieldResistance = ball.Owner.IsShielding ? 0.16f : 0f;
            var chance = Mathf.Clamp(
                Mathf.Lerp(0.36f, 0.78f, stamina) + defenderBonus + skillBonus + approachBonus -
                carrierResistance - shieldResistance,
                0f, 0.9f);
            if (Random.value > chance)
            {
                var carrier = ball.Owner;
                var carrierForward = carrier.LastMoveDirection.sqrMagnitude > 0.01f
                    ? carrier.LastMoveDirection.normalized
                    : carrier.transform.forward;
                var tacklerFromCarrier = transform.position - carrier.transform.position;
                tacklerFromCarrier.y = 0f;
                var challengeFromBehind = Vector3.Dot(carrierForward, tacklerFromCarrier) < -0.15f;
                var foulChance = Mathf.Lerp(0.14f, 0.05f, defendingStat);
                if (challengeFromBehind && Random.value < foulChance)
                    MatchManager3D.Instance?.AwardFoul(carrier.TeamDirection, carrier.transform.position);
                return false;
            }

            ball.Steal(this);
            MatchManager3D.Instance?.RecordTackleWon(TeamId);
            return true;
        }

        // Use the preferred foot for centered contact; use the nearby foot when the ball is clearly to one side.
        private bool SelectLeftFoot(Vector3 ballPosition)
        {
            var localX = transform.InverseTransformPoint(ballPosition).x;
            return Mathf.Abs(localX) < 0.12f ? prefersLeftFoot : localX < 0f;
        }

        public float SelectShotTargetX() => ChooseShotTargetX();

        public bool AttemptShot(bool chip = false, bool finesse = false, float? targetXOverride = null, float powerCharge01 = 0f, bool lowDriven = false)
        {
            var ball = BallController.Instance;
            var header = CanHeaderBall(ball);
            if (header && MatchManager3D.Instance != null &&
                MatchManager3D.Instance.CheckOffsideContact(this))
                return false;
            if ((!header && !CanPlayBall(ball)) || kickLock > 0f)
                return false;

            var goalZ = TeamDirection * 52.4f;
            var distance = Mathf.Abs(goalZ - ball.Position.z);
            if (distance > 44f)
                return false;

            var targetX = targetXOverride ?? ChooseShotTargetX();
            var flat = new Vector3(targetX - ball.Position.x, 0f, goalZ - ball.Position.z);
            if (flat.sqrMagnitude < 0.01f)
                return false;
            flat.Normalize();

            if (header)
                return AttemptHeaderShot(ball, goalZ, distance, targetX, flat, powerCharge01);

            var useLeftFoot = SelectLeftFoot(ball.Position);
            var footAccuracy = useLeftFoot == prefersLeftFoot ? 1f : 0.88f;
            var pressureDistance = NearestOpponentDistance(transform.position);
            // A close marker makes accurate placement harder, even for a skilled shooter.
            var pressureAccuracy = Mathf.Lerp(0.78f, 1f, Mathf.InverseLerp(1.4f, 5f, pressureDistance));
            var accuracy = Mathf.Lerp(0.55f, 0.95f, shootingStat)
                           * Mathf.Clamp01(1.18f - distance * 0.011f)
                           * Mathf.Lerp(0.82f, 1f, stamina)
                           * footAccuracy
                           * pressureAccuracy
                           * Mathf.Lerp(1f, 0.9f, Mathf.Clamp01(powerCharge01));
            var error = (1f - accuracy) * (0.10f + distance * 0.0035f);
            flat = (flat + transform.right * Random.Range(-error, error)).normalized;

            // Longer shots need more pace; BallController still enforces the global speed limit.
            var distancePower = Mathf.Lerp(1f, 1.4f, Mathf.InverseLerp(20f, 44f, distance));
            var power = Mathf.Lerp(15.5f, 26.5f, shootingStat) *
                        Mathf.Lerp(0.92f, 1.07f, Mathf.Clamp01(distance / 36f)) * distancePower *
                        Mathf.Lerp(1f, 1.2f, Mathf.Clamp01(powerCharge01));
            if (chip) power *= 0.72f;
            else if (finesse) power *= 0.95f;

            var lift = lowDriven ? 0.02f : chip ? 0.5f : finesse ? 0.11f :
                Mathf.Clamp(0.085f + distance * 0.0014f, 0.085f, 0.155f);
            var curve = finesse && !lowDriven
                ? Mathf.Sign(targetX - ball.Position.x) * (3.5f + shootingStat * 5.5f)
                : 0f;

            motion?.TriggerShot(useLeftFoot);
            ball.Kick(flat, power, lift, transform, curve);
            MatchManager3D.Instance?.RecordShot(TeamId, IsShotOnTarget(ball, goalZ));
            kickLock = 0.32f;
            return true;
        }

        private void BeginPassFollowRun(Vector3 receiverPosition)
        {
            passFollowStart = Flat(transform.position);
            var receiver = Flat(receiverPosition);
            var startProgress = passFollowStart.z * TeamDirection;
            var receiverProgress = receiver.z * TeamDirection;
            var targetProgress = Mathf.Min(
                Mathf.Max(startProgress + 5.5f, receiverProgress + 6f),
                Mathf.Min(startProgress + 14f, 48f));
            passFollowSide = Mathf.Abs(receiver.x) > 3f
                ? -Mathf.Sign(receiver.x)
                : Mathf.Abs(passFollowStart.x) > 2f ? -Mathf.Sign(passFollowStart.x) : 1f;
            passFollowTarget = new Vector3(
                Mathf.Clamp(receiver.x + passFollowSide * 3.5f, -27f, 27f),
                0f,
                targetProgress * TeamDirection);
            passFollowTime = 2.7f;
            passFollowRunActive = true;
        }

        private bool CanHeaderBall(BallController ball)
        {
            if (ball == null || IsGoalkeeper || ball.Owner != null ||
                ball.Position.y < HeaderContactMinHeight ||
                ball.Position.y > HeaderContactMaxHeight)
                return false;

            var delta = ball.Position - transform.position;
            delta.y = 0f;
            return delta.sqrMagnitude <= kickReach * kickReach;
        }

        private bool AttemptHeaderShot(BallController ball, float goalZ, float distance,
            float targetX, Vector3 flat, float powerCharge01)
        {
            var horizontalDistance = new Vector2(
                targetX - ball.Position.x, goalZ - ball.Position.z).magnitude;
            var pressureDistance = NearestOpponentDistance(transform.position);
            var contactQuality = Mathf.Clamp01(1f - Mathf.Abs(ball.Position.y - 1.55f) * 0.62f);
            var pressureAccuracy = Mathf.Lerp(0.72f, 1f, Mathf.InverseLerp(1.2f, 5f, pressureDistance));
            var accuracy = Mathf.Lerp(0.3f, 0.78f, shootingStat) * contactQuality *
                           pressureAccuracy * Mathf.Lerp(0.82f, 1f, stamina) *
                           Mathf.Lerp(1f, 0.92f, Mathf.Clamp01(powerCharge01));
            var error = (1f - accuracy) * (0.16f + distance * 0.0045f);
            var lateral = Vector3.Cross(Vector3.up, flat).normalized;
            flat = (flat + lateral * Random.Range(-error, error)).normalized;

            var horizontalSpeed = Mathf.Lerp(12.5f, 16f, shootingStat) *
                                  Mathf.Lerp(1f, 1.12f, Mathf.Clamp01(powerCharge01));
            var flightTime = Mathf.Max(0.08f, horizontalDistance / horizontalSpeed);
            var targetHeight = Mathf.Lerp(0.65f, 1.45f, shootingStat);
            var gravity = Mathf.Max(1f, -Physics.gravity.y);
            var verticalSpeed = (targetHeight - ball.Position.y +
                                 0.5f * gravity * flightTime * flightTime) / flightTime;
            var power = Mathf.Sqrt(horizontalSpeed * horizontalSpeed + verticalSpeed * verticalSpeed);
            var lift = power > 0.01f ? verticalSpeed / power : 0f;

            motion?.TriggerHeader();
            ball.Kick(flat, power, lift, transform);
            MatchManager3D.Instance?.RecordShot(TeamId, IsShotOnTarget(ball, goalZ));
            kickLock = 0.46f;
            return true;
        }

        private static bool IsShotOnTarget(BallController ball, float goalZ)
        {
            if (ball == null)
                return false;

            var velocity = ball.Velocity;
            if (Mathf.Abs(velocity.z) < 0.01f)
                return false;

            var timeToGoal = (goalZ - ball.Position.z) / velocity.z;
            if (timeToGoal <= 0f)
                return false;

            var crossingX = ball.Position.x + velocity.x * timeToGoal;
            var crossingY = ball.Position.y + velocity.y * timeToGoal +
                            0.5f * Physics.gravity.y * timeToGoal * timeToGoal;
            const float ballRadius = 0.11f;
            const float goalHalfWidth = 3.66f;
            const float goalHeight = 2.44f;
            return Mathf.Abs(crossingX) <= goalHalfWidth - ballRadius &&
                   crossingY >= ballRadius * 0.5f && crossingY <= goalHeight - ballRadius;
        }

        private float ChooseShotTargetX()
        {
            var keeperX = 0f;
            var foundKeeper = false;
            foreach (var candidate in Registry)
            {
                if (candidate == null || candidate.TeamDirection == TeamDirection || !candidate.IsGoalkeeper)
                    continue;
                keeperX = candidate.transform.position.x;
                foundKeeper = true;
                break;
            }

            var side = foundKeeper && Mathf.Abs(keeperX) > 0.2f
                ? -Mathf.Sign(keeperX)
                : Mathf.Sign(transform.position.x);
            if (side == 0f)
                side = Random.value < 0.5f ? -1f : 1f;

            // Only the user-controlled player reads directional aim input.
            // CPU shot placement must not change with the human player's movement.
            var aimInput = IsUserControlled
                ? Application.isMobilePlatform ? mobileMove.x : Input.GetAxisRaw("Horizontal")
                : 0f;
            return Mathf.Clamp(side * 2.6f + aimInput * 0.85f, -3.3f, 3.3f);
        }

        public bool AttemptCross()
        {
            var ball = BallController.Instance;
            if (!CanPlayBall(ball) || kickLock > 0f)
                return false;

            // Crosses are a wide attacking option, aimed into the box rather than at a teammate.
            var attackingProgress = transform.position.z * TeamDirection;
            if (Mathf.Abs(transform.position.x) < 14f || attackingProgress < 8f)
                return false;

            var aimX = IsUserControlled
                ? Application.isMobilePlatform ? mobileMove.x : Input.GetAxisRaw("Horizontal")
                : 0f;
            var forwardAim = IsUserControlled
                ? (Application.isMobilePlatform ? mobileMove.y : Input.GetAxisRaw("Vertical")) * TeamDirection
                : 0f;
            var targetX = Mathf.Clamp(-Mathf.Sign(transform.position.x) * 3.3f + aimX * 3.2f, -6.5f, 6.5f);
            var targetProgress = Mathf.Clamp(44f + forwardAim * 2.5f, 39f, 49f);
            var target = new Vector3(targetX, 0f, targetProgress * TeamDirection);
            var flat = new Vector3(target.x - ball.Position.x, 0f, target.z - ball.Position.z);
            if (flat.sqrMagnitude < 0.01f)
                return false;
            var distance = flat.magnitude;
            flat.Normalize();

            var pressureDistance = NearestOpponentDistance(transform.position);
            var crossError = (1f - passingStat) * 1.8f +
                             Mathf.Lerp(1.1f, 0f, Mathf.InverseLerp(1.5f, 6f, pressureDistance)) +
                             (1f - stamina) * 0.6f;
            flat = (flat + transform.right * Random.Range(-crossError * 0.035f, crossError * 0.035f)).normalized;

            var speed = Mathf.Clamp(15.5f + distance * 0.25f, 16f, 22f) *
                        Mathf.Lerp(0.92f, 1.04f, passingStat);
            var lift = Mathf.Lerp(0.32f, 0.43f, Mathf.InverseLerp(12f, 28f, distance));
            var useLeftFoot = SelectLeftFoot(ball.Position);
            motion?.TriggerPass(useLeftFoot);
            ball.Kick(flat, speed, lift, transform, Mathf.Sign(-transform.position.x) * 0.8f);
            MatchManager3D.Instance?.RegisterPassForOffside(this);
            kickLock = 0.34f;
            return true;
        }

        public bool AttemptClearance()
        {
            var ball = BallController.Instance;
            if (!CanPlayBall(ball) || kickLock > 0f)
                return false;

            // Send danger away from the middle and upfield when a defender is pressed near goal.
            var side = Mathf.Abs(transform.position.x) > 2f
                ? Mathf.Sign(transform.position.x)
                : (Random.value < 0.5f ? -1f : 1f);
            var direction = new Vector3(side * 0.48f, 0f, TeamDirection * 0.88f).normalized;
            var useLeftFoot = SelectLeftFoot(ball.Position);
            motion?.TriggerPass(useLeftFoot);
            ball.Kick(direction, Mathf.Lerp(19f, 22f, physicalStat), 0.22f, transform, 0f);
            MatchManager3D.Instance?.RegisterPassForOffside(this);
            kickLock = 0.42f;
            return true;
        }

        public bool AttemptPass(bool throughBall, bool lofted = false, bool dinked = false, float dinkPower01 = 0f)
        {
            var ball = BallController.Instance;
            if (!CanPlayBall(ball) || kickLock > 0f)
                return false;

            if (dinked)
                return AttemptManualDinkedPass(ball, dinkPower01);

            var target = FindPassTarget(throughBall, lofted);
            if (target == null)
                return false;

            var targetBody = target.Body;
            var flatDistance = Vector2.Distance(new Vector2(ball.Position.x, ball.Position.z),
                new Vector2(target.transform.position.x, target.transform.position.z));
            var speed = Mathf.Clamp(7f + flatDistance * 0.95f, 8.5f, 22f) * Mathf.Lerp(0.9f, 1.05f, passingStat);
            if (throughBall)
                speed *= 1.12f;
            else if (lofted)
                speed *= 1.16f;

            var aimPoint = target.transform.position;
            if (throughBall)
                aimPoint += new Vector3(0f, 0f, TeamDirection * 5.5f);
            if (targetBody != null)
                aimPoint += new Vector3(targetBody.linearVelocity.x, 0f, targetBody.linearVelocity.z) *
                            (throughBall ? 0.55f : Mathf.Clamp01(flatDistance / speed) * 0.85f);

            var flat = new Vector3(aimPoint.x - ball.Position.x, 0f, aimPoint.z - ball.Position.z);
            if (flat.sqrMagnitude < 0.01f)
                return false;
            flat.Normalize();

            var useLeftFoot = SelectLeftFoot(ball.Position);
            var footErrorFactor = useLeftFoot == prefersLeftFoot ? 1f : 1.25f;
            var pressureDistance = NearestOpponentDistance(transform.position);
            // Nearby defenders add a small directional error while passing skill still shapes the base accuracy.
            var pressureError = Mathf.Lerp(0.025f, 0f, Mathf.InverseLerp(1.25f, 5f, pressureDistance));
            var skillError = (1f - passingStat) * (throughBall ? 0.045f : lofted ? 0.04f : 0.03f);
            // Fatigue affects touch and execution, with passing skill softening the accuracy loss.
            var fatigueError = (1f - stamina) * Mathf.Lerp(0.025f, 0.012f, passingStat);
            var passError = (skillError + pressureError + fatigueError) * footErrorFactor;
            flat = (flat + transform.right * Random.Range(-passError, passError)).normalized;

            motion?.TriggerPass(useLeftFoot);
            var lift = throughBall ? 0.17f : lofted ? 0.25f : 0.025f;
            var curve = throughBall ? 1.5f : lofted ? 0.45f : 0f;
            ball.Kick(flat, speed, lift, transform, curve);
            if (IsUserControlled && passFollowArmed)
            {
                passFollowArmed = false;
                BeginPassFollowRun(target.transform.position);
            }
            MatchManager3D.Instance?.RegisterPassForOffside(this);
            kickLock = 0.26f;
            return true;
        }

        private bool AttemptManualDinkedPass(BallController ball, float power01)
        {
            var aim = ReadMove();
            var direction = aim.sqrMagnitude > 0.04f
                ? new Vector3(aim.x, 0f, aim.y).normalized
                : lastMoveDirection;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f)
                direction = new Vector3(0f, 0f, TeamDirection);
            direction.Normalize();

            var chargedPower = Mathf.Clamp01(power01);
            var speed = Mathf.Lerp(8.5f, 22f, chargedPower) *
                        Mathf.Lerp(0.9f, 1.05f, passingStat);
            var useLeftFoot = SelectLeftFoot(ball.Position);
            var footErrorFactor = useLeftFoot == prefersLeftFoot ? 1f : 1.25f;
            var pressureDistance = NearestOpponentDistance(transform.position);
            var pressureError = Mathf.Lerp(0.025f, 0f, Mathf.InverseLerp(1.25f, 5f, pressureDistance));
            var skillError = (1f - passingStat) * 0.035f;
            var fatigueError = (1f - stamina) * Mathf.Lerp(0.025f, 0.012f, passingStat);
            var passError = (skillError + pressureError + fatigueError) * footErrorFactor;
            direction = (direction + transform.right * Random.Range(-passError, passError)).normalized;

            // A charged manual dink travels farther while keeping a shallow arc.
            var distance = Mathf.Lerp(7f, 26f, chargedPower);
            var targetPoint = ball.Position + direction * distance;
            motion?.TriggerPass(useLeftFoot);
            ball.Kick(direction, speed, 0.21f, transform, 0.08f);
            if (IsUserControlled && passFollowArmed)
            {
                passFollowArmed = false;
                BeginPassFollowRun(targetPoint);
            }
            MatchManager3D.Instance?.RegisterPassForOffside(this);
            kickLock = 0.26f;
            return true;
        }

        private FootballPlayerController FindPassTarget(bool throughBall, bool lofted)
        {
            var ball = BallController.Instance;
            var origin = ball != null ? ball.Position : transform.position;
            var forward = lastMoveDirection.sqrMagnitude > 0.01f
                ? lastMoveDirection.normalized
                : new Vector3(0f, 0f, TeamDirection);

            FootballPlayerController best = null;
            var bestScore = float.MinValue;
            var maxRange = throughBall ? 32f : lofted ? 32f : 26f;

            foreach (var candidate in Registry)
            {
                // Through balls need a runner ahead; regular passes may use the goalkeeper as a safe outlet.
                if (candidate == null || candidate == this || candidate.TeamDirection != TeamDirection ||
                    (throughBall && candidate.IsGoalkeeper))
                {
                    continue;
                }

                var offset = candidate.transform.position - origin;
                offset.y = 0f;
                var distance = offset.magnitude;
                if (distance < 3.5f || distance > maxRange)
                    continue;
                if (candidate.transform.position.z * TeamDirection < transform.position.z * TeamDirection - 12f)
                    continue;

                var forwardAmount = Vector3.Dot(offset.normalized, forward);
                if (forwardAmount < (throughBall ? 0.2f : -0.25f))
                    continue;

                var openness = NearestOpponentDistance(candidate.transform.position);
                var laneRisk = LaneRisk(origin, candidate.transform.position);
                // Lofted passes can clear a tight ground lane, but close marking still lowers the target's score.
                var laneRiskPenalty = lofted
                    ? Mathf.Lerp(4.5f, 2.5f, passingStat)
                    : Mathf.Lerp(9f, 5f, passingStat);
                var score = forwardAmount * 9f + Mathf.Clamp(openness, 0f, 12f) * 0.85f
                            - Mathf.Abs(offset.x) * 0.07f - distance * 0.08f - laneRisk * laneRiskPenalty;

                if (throughBall)
                    score += offset.z * TeamDirection * 0.12f;
                if (candidate.IsGoalkeeper)
                    score -= 1.5f;
                if (candidate.Role == PlayerRole.Forward)
                    score += throughBall ? 3f : 1.2f;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private float NearestOpponentDistance(Vector3 position)
        {
            var nearest = 20f;
            foreach (var candidate in Registry)
            {
                if (candidate == null || candidate.TeamDirection == TeamDirection)
                    continue;
                var delta = candidate.transform.position - position;
                delta.y = 0f;
                nearest = Mathf.Min(nearest, delta.magnitude);
            }
            return nearest;
        }

        private float LaneRisk(Vector3 from, Vector3 to)
        {
            var start = new Vector2(from.x, from.z);
            var end = new Vector2(to.x, to.z);
            var length = Vector2.Distance(start, end);
            if (length < 0.5f)
                return 0f;

            var risk = 0f;
            foreach (var candidate in Registry)
            {
                if (candidate == null || candidate.TeamDirection == TeamDirection)
                    continue;

                var point = new Vector2(candidate.transform.position.x, candidate.transform.position.z);
                var t = Mathf.Clamp01(Vector2.Dot(point - start, end - start) / (length * length));
                var distance = Vector2.Distance(point, Vector2.Lerp(start, end, t));
                if (distance < 2.4f)
                    risk += Mathf.Lerp(1f, 0.25f, t) * (1f - distance / 2.4f);
            }
            return risk;
        }

        private bool CanPlayBall(BallController ball)
        {
            if (ball == null)
                return false;
            if (ball.Owner == this)
                return true;
            if (ball.Owner != null || !ball.CanBeControlledBy(this))
                return false;

            var delta = ball.Position - transform.position;
            delta.y = 0f;
            return delta.sqrMagnitude <= kickReach * kickReach;
        }
    }
}
