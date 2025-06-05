using UnityEngine;
using GFD.Map;

public class LocationDescription : MonoBehaviour
{
    [SerializeField]
    private LocationDetail locationDetail;

    public LocationDetail LocationDetail
    {
        get { return locationDetail; }
        set { locationDetail = value; }
    }
}
