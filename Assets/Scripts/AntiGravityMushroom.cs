using UnityEngine;

public class AntiGravityMushroom : MonoBehaviour
{
    [field: SerializeField]
    public float Width { get; private set; } = 3;
    [field: SerializeField]
    public float Height { get; private set; } = 10;

    private GameObject[] _debugQuads;

    private void Awake()
    {
        Init();
    }

    private void OnValidate()
    {
        Init();
    }

    private void Init()
    {
        var offsetGO = transform.GetChild(0);
        offsetGO.localPosition = new Vector3(0, Height * 0.5F);
        offsetGO.localScale = new Vector3(Width, Height);

        _debugQuads = new GameObject[4];
        _debugQuads[0] = transform.GetChild(1).gameObject;
        _debugQuads[1] = transform.GetChild(2).gameObject;
        _debugQuads[2] = transform.GetChild(3).gameObject;
        _debugQuads[3] = transform.GetChild(4).gameObject;
        _debugQuads[0].transform.localScale = new Vector3(Height, 0.05F);
        _debugQuads[0].transform.localPosition = new Vector3(-Width * 0.5F, Height * 0.5F, +50);
        _debugQuads[0].transform.localRotation = Quaternion.LookRotation(Vector3.forward, -transform.right);
        _debugQuads[1].transform.localScale = new Vector3(Width, 0.05F);
        _debugQuads[1].transform.localPosition = new Vector3(0, Height, +50);
        _debugQuads[1].transform.localRotation = Quaternion.LookRotation(Vector3.forward, transform.up);
        _debugQuads[2].transform.localScale = new Vector3(Height, 0.05F);
        _debugQuads[2].transform.localPosition = new Vector3(+Width * 0.5F, Height * 0.5F, +50);
        _debugQuads[2].transform.localRotation = Quaternion.LookRotation(Vector3.forward, transform.right);
        _debugQuads[3].transform.localScale = new Vector3(Width, 0.05F);
        _debugQuads[3].transform.localPosition = new Vector3(0, 0, +50);
        _debugQuads[3].transform.localRotation = Quaternion.LookRotation(Vector3.forward, -transform.up);

        // Set colors to red.
        foreach (var quad in _debugQuads)
        {
            quad.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerRespawned += Reset;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerRespawned -= Reset;
    }


    // TODO: If IsActive is set to true, have to retrigger collisions (overlap a box and do the same thing as in OnCollisionEnter2D).
    public bool IsActive { get; private set; } = false;

    public void Activate()
    {
        IsActive = true;

        // Set colors to blue.
        foreach (var quad in _debugQuads)
        {
            quad.GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("OnTriggerEnter2D");
        var ko = collider.GetComponent<KinematicObject>();
        if (ko == null)
        {
            return;
        }

        if (IsActive)
        {
            ko.InvertGravity = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        var controller = collider.GetComponent<PlayerController>();
        if (controller == null)
        {
            return;
        }

        if (IsActive)
        {
            controller.Ko.InvertGravity = false;
        }
    }

    private void Reset()
    {
        IsActive = false;
        
        // Set colors back to red.
        foreach (var quad in _debugQuads)
        {
            quad.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
}
