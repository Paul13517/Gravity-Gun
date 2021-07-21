using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARGravityGun : MonoBehaviour
{
    public Camera ARCamera;

    public GameObject OnButton;
    public GameObject GravityGunn;
    private GameObject RaisedObject;
    public GameObject Cube;
    public GameObject LeverArm;

    private Rigidbody RaisedObjectRB;

    private Renderer OnButtonRenderer;

    public Transform HoldPosition;

    public float attractSpeed;
    public float ThrowForce;

    private bool PressedButton = false;
    private bool RaiseObject = false;
    private bool ObjectRotation = false;


    // Start is called before the first frame update
    void Start()
    {
        OnButtonRenderer = OnButton.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Отслеживание нажатия пальца на экран
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

           // Запуск информационного луча места нажатия пальца на экран
            Ray ray = ARCamera.ScreenPointToRay(touch.position);
            RaycastHit hitbutton;

           // Проверка пересечения луча с кнопкой
            if (Input.touches[0].phase == TouchPhase.Began && Physics.Raycast(ray, out hitbutton) && hitbutton.transform.name == "OnButton" && !PressedButton)
            {
                PressedButton = true;
                OnButtonRenderer.material.SetColor("_Color", Color.green);
            }
            // Проверка отжатия кнопки
            else if (Input.touches[0].phase == TouchPhase.Ended && PressedButton)
            {
                OnButtonRenderer.material.SetColor("_Color", Color.red);
                PressedButton = false;
            }

            if (PressedButton)
            {
                // Функция по захвату объекта
                TakeObject(touch);
            }

            // Проверка на перемещение пальца по экрану
            if (touch.phase == TouchPhase.Moved)
            {
                float LastTouch = Vector2.Distance(touch.position, new Vector2(0, 0));
                float PrevTouch = Vector2.Distance(touch.position, touch.deltaPosition);

                // Расчет расстояния между предыдущим и нынешним положениям пальца на экране
                float Delta = LastTouch - PrevTouch;

                if (Delta > 0 && RaiseObject && !ObjectRotation)
                {
                    ShootObj();
                }
                else if (Delta < 0 && RaiseObject && !ObjectRotation)
                {
                    DropObj();
                }
                else if (Delta != 0 && RaiseObject && ObjectRotation)
                {
                    RotateObj(Delta);
                }

            }

            if (Input.touches[0].phase == TouchPhase.Began && Physics.Raycast(ray, out hitbutton) && hitbutton.transform.name == "LeverArm" && RaiseObject)
            {
                ObjectRotation = true;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended && ObjectRotation)
            {
                ObjectRotation = false;
            }

        }

        if (RaiseObject && CheckDist() >= 0.1f)
        {
            MoveObjToPos();
        }
    }

    private void TakeObject(Touch touch)
    {
       // Запуск информационного луча из гравити пушки
        Ray mainray = new Ray(GravityGunn.transform.position, GravityGunn.transform.forward);
        RaycastHit hitObject;

        // Проверка пересечения луча с виртуальным объектом
        if (Physics.Raycast(mainray, out hitObject) && hitObject.transform.tag == "GameObject" && touch.phase == TouchPhase.Stationary && !RaiseObject)
        {
            RaisedObject = hitObject.collider.gameObject;
            RaisedObject.transform.SetParent(HoldPosition);

            RaisedObjectRB = RaisedObject.GetComponent<Rigidbody>();
            RaisedObjectRB.constraints = RigidbodyConstraints.FreezeAll;

            RaiseObject = true;

            OnButtonRenderer.material.SetColor("_Color", Color.red);
            PressedButton = false;
        }
    }

    public float CheckDist()
    {
        float dist = Vector3.Distance(RaisedObject.transform.position, HoldPosition.transform.position);
        return dist;
    }

    private void MoveObjToPos()
    {
        RaisedObject.transform.position = Vector3.Lerp(RaisedObject.transform.position, HoldPosition.position, attractSpeed * Time.deltaTime);
    }

    public void SpawnBox()
    {
        Instantiate(Cube, ARCamera.transform.forward * 2, Cube.transform.rotation);
    }

    public void DropObj()
    {
        RaisedObjectRB.constraints = RigidbodyConstraints.None;
        RaisedObject.transform.parent = null;
        RaisedObject = null;
        RaiseObject = false;
    }

    private void ShootObj()
    {
        RaisedObjectRB.AddForce(ARCamera.transform.forward * ThrowForce, ForceMode.Impulse);
        DropObj();
    }

    private void RotateObj(float Delta)
    {
        RaisedObject.transform.Rotate(0f, -Delta * 1, 0f);
        LeverArm.transform.Rotate(-Delta * 1, 0f, 0f);
    }
}
