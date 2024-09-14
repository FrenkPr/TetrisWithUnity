using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    /*
     * DEFAULT TIME SETTINGS
     * 
     * Fixed timestep: 0.02
     * max allowed timestep: 0.3333333
     * max particle timestep: 0.03
     */

    public float moveSpeed;
    public float moveOneCellSpeed;
    private Vector3 velocity;

    [SerializeField] private float lateralMoveTimestep;
    [SerializeField] private float maxFallingTimestep;
    [SerializeField] private float minFallingTimestep;
    [SerializeField] private List<Color> colors;

    private bool setPause;

    // Start is called before the first frame update
    void Start()
    {
        Color randColor = colors[UnityEngine.Random.Range(0, colors.Count)];

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<SpriteRenderer>().color = randColor;
        }

        StartCoroutine("LateralMoveInput", lateralMoveTimestep);
        StartCoroutine("LateralMove", lateralMoveTimestep);

        setPause = true;
    }

    public void TogglePause()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = Convert.ToInt32(!setPause);
            setPause = !setPause;
        }
    }

    public void RotateBlock()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 oldPosition = transform.position;

            transform.eulerAngles += new Vector3(0, 0, -90);

            //we check 4 times (num of cells) if the block is colliding to left after rotation
            if (GridMngr.Instance.IsCurrentBlockCollidingLeft())
            {
                //print("colliding to left on rotation");

                MoveToRightOfOne();

                ////we check other 3 times if the block is still colliding after rotation
                for (int i = 0; i < transform.childCount - 1; i++)
                {
                    if (GridMngr.Instance.IsCurrentBlockCollidingLeft())
                    {
                        //print("move to right more than one time");

                        MoveToRightOfOne();
                    }
                }
            }

            //we check 4 times (num of cells) if the block is colliding to right after rotation
            if (GridMngr.Instance.IsCurrentBlockCollidingRight())
            {
                //print("colliding to right on rotation");

                MoveToLeftOfOne();

                ////we check other 3 times if the block is still colliding after rotation
                for (int i = 0; i < transform.childCount - 1; i++)
                {
                    if (GridMngr.Instance.IsCurrentBlockCollidingRight())
                    {
                        //print("move to left more than one time");

                        MoveToLeftOfOne();
                    }
                }
            }

            //we check 4 times (num of cells) if the block is colliding down after rotation
            if (GridMngr.Instance.IsCurrentBlockCollidingDown())
            {
                //print("colliding down on rotation");

                MoveUpOfOne();

                ////we check other 3 times if the block is still colliding after rotation
                for (int i = 0; i < transform.childCount - 1; i++)
                {
                    if (GridMngr.Instance.IsCurrentBlockCollidingDown())
                    {
                        //print("move up more than one time");

                        MoveUpOfOne();
                    }
                }
            }

            //we check, after rotation and moving the block of 1 cell to right, left or bottom, in case of collision,
            //if the block is out of bounds. Sometimes it may happen
            if (IsBlockOutOfBounds())
            {
                transform.eulerAngles += new Vector3(0, 0, 90);
                transform.position = oldPosition;
            }
        }
    }

    public IEnumerator LateralMoveInput(float delay)
    {
        for (; ; )
        {
            if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            {
                velocity.x = 0;
                yield return null;
            }

            float dirX = 0;

            if (Input.GetKey(KeyCode.A) && !GridMngr.Instance.IsCurrentBlockCollidingLeft())
            {
                dirX = -1;
            }

            else if (Input.GetKey(KeyCode.D) && !GridMngr.Instance.IsCurrentBlockCollidingRight())
            {
                dirX = 1;
            }

            velocity.x = moveSpeed * dirX;

            yield return new WaitForSeconds(delay);
        }
    }

    public void LetItFallFasterInput()
    {
        if (!Input.GetKey(KeyCode.S))
        {
            Time.fixedDeltaTime = maxFallingTimestep;
            Time.maximumDeltaTime = maxFallingTimestep;

            return;
        }

        Time.fixedDeltaTime -= Time.deltaTime * 2;
        Time.maximumDeltaTime -= Time.deltaTime * 2;

        Time.fixedDeltaTime = Mathf.Clamp(Time.fixedDeltaTime, minFallingTimestep, maxFallingTimestep);
        Time.maximumDeltaTime = Mathf.Clamp(Time.maximumDeltaTime, minFallingTimestep, maxFallingTimestep);
    }

    public Transform[] UnpackBlock(Transform cellsParent)
    {
        Transform[] childrenBlock = new Transform[transform.childCount];

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            childrenBlock[i] = transform.GetChild(i);
            transform.GetChild(i).parent = cellsParent;

            childrenBlock[i].rotation = Quaternion.identity;
            childrenBlock[i].localScale = Vector3.one * 0.5f;
        }

        Destroy(this);

        return childrenBlock;
    }

    public bool IsBlockOutOfBounds()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).position.x < GridMngr.Instance.MinGridBounds.x ||
                transform.GetChild(i).position.x > GridMngr.Instance.MaxGridBounds.x ||
                transform.GetChild(i).position.y > GridMngr.Instance.MinGridBounds.y ||
                transform.GetChild(i).position.y < GridMngr.Instance.MaxGridBounds.y)
            {
                return true;
            }
        }

        return false;
    }

    public void MoveUpOfOne()
    {
        transform.position += new Vector3(0, moveOneCellSpeed, 0);
    }

    public void MoveToLeftOfOne()
    {
        transform.position += new Vector3(-moveOneCellSpeed, 0, 0);
    }

    public void MoveToRightOfOne()
    {
        transform.position += new Vector3(moveOneCellSpeed, 0, 0);
    }

    public IEnumerator LateralMove(float delay)
    {
        for (; ; )
        {
            transform.position += velocity;
            yield return new WaitForSeconds(delay);
        }
    }

    public void FallingMove()
    {
        transform.position += new Vector3(0, -moveSpeed, 0);
    }

    // Update is called once per frame
    void Update()
    {
        TogglePause();

        RotateBlock();
        LetItFallFasterInput();
    }

    private void FixedUpdate()
    {
        //velocity.x = moveSpeed;
        //velocity.y = -moveSpeed;

        //LateralMoveInput();
        FallingMove();
    }
}
