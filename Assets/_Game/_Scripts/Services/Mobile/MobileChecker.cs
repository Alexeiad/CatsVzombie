

using UnityEngine;

public class MobileChecker : MonoBehaviour
{
    public static bool IsMobileDevice { get; private set; }

    [SerializeField] private bool _isMobileDevice;


    private void Awake()
    {
        if (Application.isMobilePlatform || _isMobileDevice)
            IsMobileDevice = true;
    }

}

    

