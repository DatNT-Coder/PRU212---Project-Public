using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ParticleSystem dustTrail;
	[SerializeField] private Health health;
	[SerializeField] private TMP_Text skill1Text;

	[Header("Settings")]
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float turningRate = 30f;
    [SerializeField] private float emissionRate = 10f;

	// Biến cooldown
	private float skill1Cooldown = 10f; // Thời gian hồi cho kỹ năng 1
	private float skill2Cooldown = 20f;  // Thời gian hồi cho kỹ năng 2
	private float skill3Cooldown = 100f;  // Thời gian hồi cho kỹ năng 3

	private float skill1CooldownTimer = 0f; // Thời gian còn lại cho kỹ năng 1
	private float skill2CooldownTimer = 0f; // Thời gian còn lại cho kỹ năng 2
	private float skill3CooldownTimer = 0f; // Thời gian còn lại cho kỹ năng 3

	private ParticleSystem.EmissionModule emissionModule;
    private Vector2 previousMovementInput;
    private Vector3 previousPos;

    private const float ParticleStopThreshhold = 0.005f;

	private ProjectileLauncher projectileLauncher;

	private void Awake()
    {
        emissionModule = dustTrail.emission;
		// Lấy tham chiếu đến ProjectileLauncher
		projectileLauncher = GetComponent<ProjectileLauncher>();

		// Kiểm tra xem đã lấy được tham chiếu chưa
		if (projectileLauncher == null)
		{
			Debug.LogError("ProjectileLauncher is not found on the same GameObject!");
		}

		health = GetComponent<Health>();
	}

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        inputReader.MoveEvent += HandleMove;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        inputReader.MoveEvent -= HandleMove;
    }

    private void Update()
    {
		if (!IsOwner) { return; }

		// Giảm thời gian hồi cho các kỹ năng
		if (skill1CooldownTimer > 0)
		{
			skill1CooldownTimer -= Time.deltaTime;
		}

		if (skill2CooldownTimer > 0)
		{
			skill2CooldownTimer -= Time.deltaTime;
		}

		if (skill3CooldownTimer > 0)
		{
			skill3CooldownTimer -= Time.deltaTime;
		}

		float zRotation = previousMovementInput.x * -turningRate * Time.deltaTime;
		bodyTransform.Rotate(0f, 0f, zRotation);

		// Kiểm tra các phím kỹ năng
		if (Input.GetKeyDown(KeyCode.Alpha1) && skill1CooldownTimer <= 0)
		{
			UseSkill1();
			skill1CooldownTimer = skill1Cooldown; // Reset cooldown cho kỹ năng 1
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2) && skill2CooldownTimer <= 0)
		{
			UseSkill2();
			skill2CooldownTimer = skill2Cooldown; // Reset cooldown cho kỹ năng 2
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3) && skill3CooldownTimer <= 0)
		{
			UseSkill3();
			skill3CooldownTimer = skill3Cooldown; // Reset cooldown cho kỹ năng 3
		}
	}

    private void FixedUpdate()
    {
        if ((transform.position - previousPos).sqrMagnitude > ParticleStopThreshhold)
        {
            emissionModule.rateOverTime = emissionRate;
        }
        else
        {
            emissionModule.rateOverTime = 0;
        }

        previousPos = transform.position;

        if (!IsOwner) { return; }

        rb.velocity = (Vector2)bodyTransform.up * previousMovementInput.y * movementSpeed;
    }

    private void HandleMove(Vector2 movementInput)
    {
        previousMovementInput = movementInput;
    }
	// Các hàm kỹ năng
	private void UseSkill1()
	{
		Debug.Log("Kỹ năng 1 được kích hoạt!");
		// Thêm logic cho kỹ năng 1 ở đây
		StartCoroutine(SpeedBoostCoroutine());

	}
	private IEnumerator SpeedBoostCoroutine()
	{
		float originalSpeed = movementSpeed;
		movementSpeed *= 2;  // Tăng tốc độ x2
		yield return new WaitForSeconds(5f);  // Chờ 5 giây
		movementSpeed = originalSpeed;  // Trả tốc độ về giá trị ban đầu
		Debug.Log("Kỹ năng 1 đã kết thúc!");
	}
	private void UseSkill2()
	{
		Debug.Log("Kỹ năng 2 được kích hoạt!");
		// Thêm logic cho kỹ năng 2 ở đây
		StartCoroutine(ProjectileSpeedBoostCoroutine());

	}

	private IEnumerator ProjectileSpeedBoostCoroutine()
	{
		float originalProjectileSpeed = projectileLauncher.GetProjectileSpeed(); // Lưu tốc độ projectile hiện tại
		projectileLauncher.SetProjectileSpeed(originalProjectileSpeed * 2f); // Tăng tốc độ projectile

		yield return new WaitForSeconds(5f); // Chờ 5 giây

		projectileLauncher.SetProjectileSpeed(originalProjectileSpeed); // Khôi phục tốc độ projectile
		Debug.Log("Kỹ năng 2 đã kết thúc!");
	}

	private void UseSkill3()
	{
		Debug.Log("Kỹ năng 3 được kích hoạt!");
		// Thêm logic cho kỹ năng 3 ở đây
		// Hồi máu cho nhân vật khi nhấn nút 3
		if (health != null)
		{
			// Kiểm tra xem máu hiện tại có bằng tối đa không
			if (health.CurrentPlayerHealth.Value >= health.MaxHealth)
			{
				Debug.Log("Nhân vật đã đầy máu!");
			}
			else
			{
				health.RestoreHealth(20); // Thay 20 bằng giá trị hồi máu bạn muốn
				Debug.Log("Nhân vật được hồi máu!");
			}
		}
		else
		{
			Debug.LogError("Health component is not assigned!");
		}
	}
}