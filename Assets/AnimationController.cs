using UnityEngine;

public delegate void AnimationFinished();

[RequireComponent(typeof(Animation))]
public class AnimationController : MonoBehaviour
{
    Animation mainAnimation;    
    int mainAnimationFrames;
    int currentFrame;
    public float AnimationDuration { get; private set; }

    public event AnimationFinished AnimationFinished;

    void OnDestroy()
    {
        mainAnimation = null;
        AnimationFinished?.Invoke();
    }

    public void Init()
    {
        mainAnimation = GetComponent<Animation>();
        AnimationDuration = mainAnimation.clip.length;
        mainAnimationFrames = Mathf.RoundToInt(mainAnimation.clip.frameRate * AnimationDuration);
        currentFrame = 0;
        mainAnimation?.clip?.SampleAnimation(this.gameObject, GetTimeByFrame(currentFrame));
    }

    public void NextFrame()
    {
        currentFrame++;
        mainAnimation?.clip?.SampleAnimation(this.gameObject, GetTimeByFrame(currentFrame));

        if(currentFrame >= mainAnimationFrames)
        {
            AnimationFinished?.Invoke();
        }
    }

    public void PreviousFrame()
    {
        currentFrame--;
        mainAnimation?.clip?.SampleAnimation(this.gameObject, GetTimeByFrame(currentFrame));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Init();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            NextFrame();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            PreviousFrame();
        }
    }

    float GetTimeByFrame(int currentFrame)
    {
        return currentFrame * AnimationDuration / mainAnimationFrames;
    }
}

