using UnityEngine;
using UnityEngine.SceneManagement;

public class Movement : MonoBehaviour {

    [SerializeField] float rocketThrust = 100f;
    [SerializeField] float mainThrust = 1f;

    [SerializeField] float levelLoadDelay =2f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip destroyed;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem destroyedParticles;

    Rigidbody rigidBody;
    AudioSource audioSource;

    enum State { Alive, Dead, Transcending };
    State state = State.Alive;

    bool collisionsDisabled = false;

	// Use this for initialization
	void Start () {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {

        if (state == State.Alive)
        {
            RespondToThrustInput();
            RespondToStearInput();
        }

        if (Debug.isDebugBuild) { respondToDebugActions(); }
	}

    // immediately load next level on "L" key pressed
    private void respondToDebugActions()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextScene();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            collisionsDisabled = !collisionsDisabled;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || collisionsDisabled) { return; } // ignore collisions when dead

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                print("Doing fine");
                break;
            case "Finish":
                LoadNextLevel();
                break;
            default:
                LoadFirstLevel();
                break;
        }
    }

    private void LoadFirstLevel()
    {
        state = State.Dead;
        audioSource.Stop();
        audioSource.PlayOneShot(destroyed);
        destroyedParticles.Play();
        Invoke("RestartGame", levelLoadDelay);
    }

    private void LoadNextLevel()
    {
        state = State.Transcending;
        audioSource.Stop();
        audioSource.PlayOneShot(success);
        successParticles.Play();
        Invoke("LoadNextScene", levelLoadDelay);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0; //loop back to start
        }
       SceneManager.LoadScene(nextSceneIndex);
    }

    private void skipLevel() {
        if (Input.GetKey(KeyCode.L))
        {
            SceneManager.LoadScene(1);
        }
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ThrustEngine();
        }
        else
            
        {
            StopThrust();
        }
    }

    private void StopThrust()
    {
        audioSource.Stop();
        mainEngineParticles.Stop();
    }

    private void ThrustEngine()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust);

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
        }
        mainEngineParticles.Play();
    }

    private void RespondToStearInput()
    {
        rigidBody.angularVelocity = Vector3.zero; // remove rotation due to physics reason

        float rotationFrame = rocketThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.forward * rotationFrame);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(-Vector3.forward * rotationFrame);
        }
    }
}
