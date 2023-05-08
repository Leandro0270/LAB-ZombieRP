using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffScreenCameraIcons : MonoBehaviour
{
    [SerializeField] private Camera _MainCamera;
    private List<OffScreenObjectIcon> _trackedObjects = new List<OffScreenObjectIcon>();
    
    public void AddTrackedObject(OffScreenObjectIcon trackedObject)
    {
        if(!_trackedObjects.Contains(trackedObject))
            _trackedObjects.Add(trackedObject);
        else
            Debug.LogWarning("Tried to add an already tracked object");
    }
    
    public void removeTrackedObject(OffScreenObjectIcon trackedObject)
    {
        if(_trackedObjects.Contains(trackedObject))
            _trackedObjects.Remove(trackedObject);
        else
            Debug.LogWarning("Tried to remove an untracked object");
    }
    
    private bool IsVisible( GameObject go ) {
        Plane [] planes = GeometryUtility.CalculateFrustumPlanes( _MainCamera );
        Renderer [] rends = go.GetComponentsInChildren<Renderer>();
        foreach ( var r in rends ) {
            if ( GeometryUtility.TestPlanesAABB( planes, r.bounds ) ) {
                return true;
            }
        }
        return false;
    }
    
    private Vector3 FixBehindCamera( Vector3 p ) {
        Vector3 d = p - _MainCamera.transform.position;
        Vector3 n = _MainCamera.transform.forward;
        float ds = Vector3.Dot( d, n );
        if ( ds > 0 ) {
            return p;
        }
        // make sure that we are never on the camera plane
        if ( ds == 0 ) {
            return p + n * 0.00001f;
        }
        return p - 2 * ds * n;
    }
        void OnGUI() {
        if ( ! Event.current.type.Equals( EventType.Repaint ) ) {
            return;
        }
        Rect camRect = _MainCamera.pixelRect;
        Vector2 iconSize = new Vector2( camRect.height / 14, camRect.height / 14 );
        Vector2 iconExt = iconSize / 2;
        Vector2 arrowSize = new Vector2( iconSize.x / 4, iconSize.y / 2 );
        Vector2 arrowExt = arrowSize / 2;
        Matrix4x4 mt = new Matrix4x4();
        mt.SetColumn(0, new Vector3( 1, 0, 0 ) );
        mt.SetColumn(1, new Vector3( 0, 1, 0 ) );
        mt.SetColumn(2, new Vector3( 0, 0, 1 ) );
        mt.SetColumn(3, new Vector4( -arrowExt.x, -arrowExt.y, 0, 1 ) );
        float margin = arrowSize.y;
        for ( int i = _trackedObjects.Count - 1; i >= 0; i-- ) {
            OffScreenObjectIcon marker = _trackedObjects[i];
            if ( marker && marker.gameObject ) {
                if ( ! IsVisible( marker.gameObject ) ) {
                    Vector3 wp = FixBehindCamera( marker.transform.position );
                    Vector2 mrkScrPos = _MainCamera.WorldToScreenPoint( wp );
                    mrkScrPos.y = camRect.height - mrkScrPos.y;
                    Vector2 iconPos = new Vector2(
                        Mathf.Clamp( mrkScrPos.x, iconExt.x + margin, camRect.width - iconExt.x - margin ),
                        Mathf.Clamp( mrkScrPos.y, iconExt.y + margin, camRect.height - iconExt.y - margin )
                    );
                    Rect ri = new Rect( iconPos.x - iconExt.x, iconPos.y - iconExt.y, iconSize.x, iconSize.y );
                    GUI.DrawTexture( ri, marker.Icon, ScaleMode.StretchToFill, alphaBlend: true, imageAspect: 0, 
                                            color: marker.Color, borderWidth: 0, borderRadius: 0); 
                    Vector2 towardMrk = mrkScrPos - iconPos;
                    if ( towardMrk.sqrMagnitude > 0.001f ) {
                        Vector2 twn = towardMrk.normalized;
                        Vector2 arrowPos = iconPos + twn * ( iconExt.x + arrowExt.y );
                        Matrix4x4 oldm = GUI.matrix;
                        Matrix4x4 mr = new Matrix4x4();
                        mr.SetColumn(0, new Vector3( twn.y, -twn.x, 0 ) );
                        mr.SetColumn(1, new Vector3( -twn.x, -twn.y, 0 ) );
                        mr.SetColumn(2, new Vector3( 0, 0, 1 ) );
                        mr.SetColumn(3, new Vector4( arrowPos.x, arrowPos.y, 0, 1 ) );
                        GUI.matrix = mr * mt;
                        Rect ra = new Rect( 0, 0, arrowSize.x, arrowSize.y );
                        GUI.DrawTexture( ra, marker.Arrow, ScaleMode.StretchToFill, alphaBlend: true, imageAspect: 0, 
                                            color: marker.Color, borderWidth: 0, borderRadius: 0); 
                        GUI.matrix = oldm;
                    }
                }
            } else {
                _trackedObjects.RemoveAt( i );
            }
        }
    }

}

