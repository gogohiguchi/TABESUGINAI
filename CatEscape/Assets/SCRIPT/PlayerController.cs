using UnityEngine;



public class PlayerController : MonoBehaviour

{

    public float moveSpeed = 10f;



    void Update()

    {

        float moveX = 0;



        if (Input.GetKey(KeyCode.LeftArrow)) moveX = -2;

        if (Input.GetKey(KeyCode.RightArrow)) moveX = 2;



        transform.Translate(moveX * moveSpeed * Time.deltaTime, 0, 0);

    }

}