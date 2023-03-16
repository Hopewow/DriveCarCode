using System;
using UnityEngine;

[RequireComponent(typeof(NNet))]
public class CarController : MonoBehaviour
{
    // Used to set restart point on the track
    public Vector3 startPosition, startRotation;
    private NNet network;

    [Range(-1f, 1f)]
    public float speed, turning;

    public float timeSinceStart = 0f;

    [Header("Fitness")] // Basicly loss
    public float overallFitness;
    /*
     * We will in this case prioritise distance over speed so it does not drive at march 10 instead of finishing the track.
     */
    public float distanceMultiplier = 1.4f; // How important distance is
    public float avgSpeedMultiplier = 0.1f; // How important speed is
    public float sensorMultiplier = 0.1f; // How important sensor distance is

    [Header("Network Options")]
    public int[] layers = new int[3] { 3, 3, 2 };//initializing network to the right size

    // These are the general variable on each car used to calculate the fitness/loss
    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    // Distance value of each sensor
    private float aSensor, bSensor, cSensor;

    private void Start()
    {
        transform.position = new Vector3(-56.1f, 8.58f, 12.6f);
        transform.rotation = new Quaternion(0f, 146.58f, 0f, 45f);
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
    }

    public void ResetWithNetwork(NNet net)
    {
        network = net;
        Reset();
    }

    private void Reset()
    {
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    /*
     * When the car collides with anything.
     */
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            return;
        }
        Death();
    }

    private void FixedUpdate()
    {
        InputSensors();
        lastPosition = transform.position;

        // Neural network here for how much to speed up and turn
        (speed, turning) = network.RunNetwork(aSensor, bSensor, cSensor);

        //print("Speed: "+speed+" turning: "+turning);

        MoveCar(speed, turning);

        timeSinceStart += Time.deltaTime;

        fitness();
    }

    private void Death()
    {
        GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness, network);
    }

    /*
     * Used to calculate the fitness/loss how well it did.
     */
    private void fitness()
    {
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;


        overallFitness = (totalDistanceTravelled * distanceMultiplier) + (avgSpeed * avgSpeedMultiplier) + (((aSensor + bSensor + cSensor) / 3) * sensorMultiplier);

        if (timeSinceStart > 20 && overallFitness < 40)
        {
            Death();
        }

        if (overallFitness >= 1500)
        {
            // Saves a working car data.
            Death();
        }
    }

    /*
     * Sets the sensors on the car to tell us how far away from the walls we are.
     * Distances are divided by 20 so that we are getting a value of between 0 and 1 for the sigmund function.
     */
    private void InputSensors()
    {
        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit))
        {
            aSensor = hit.distance / 20;
        }

        r.direction = b;

        if (Physics.Raycast(r, out hit))
        {
            bSensor = hit.distance / 20;
        }

        r.direction = c;

        if (Physics.Raycast(r, out hit))
        {
            cSensor = hit.distance / 20;
        }
    }

    /*
     * This function adds some direction to the current direction of the car.
     */
    private Vector3 inp; // input
    private void MoveCar(float speed, float turning)
    {
        inp = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, speed * 11.4f), 0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp;

        transform.eulerAngles += new Vector3(0, (turning * 90) * 0.1f, 0); // Handles the turning and 0.02f is used to smooth the turning out.
    }
}
