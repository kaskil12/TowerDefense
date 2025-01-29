using Photon.Pun;
using UnityEngine;

public class CustomPhotonTransformView : MonoBehaviourPun, IPunObservable
{
    [Header("Position Sync")]
    public bool syncPosition = true;
    public bool syncPosX = true;
    public bool syncPosY = true;
    public bool syncPosZ = true;

    [Header("Rotation Sync")]
    public bool syncRotation = true;
    public bool syncRotX = true;
    public bool syncRotY = true;
    public bool syncRotZ = true;

    [Header("Scale Sync")]
    public bool syncScale = true;
    public bool syncScaleX = true;
    public bool syncScaleY = true;
    public bool syncScaleZ = true;

    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkScale;

    private void Awake()
    {
        networkPosition = transform.position;
        networkRotation = transform.rotation;
        networkScale = transform.localScale;
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            // Smooth interpolation for remote players
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Slerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
            transform.localScale = Vector3.Lerp(transform.localScale, networkScale, Time.deltaTime * 10f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Sending data
            if (syncPosition)
            {
                if (syncPosX) stream.SendNext(transform.position.x);
                if (syncPosY) stream.SendNext(transform.position.y);
                if (syncPosZ) stream.SendNext(transform.position.z);
            }

            if (syncRotation)
            {
                if (syncRotX) stream.SendNext(transform.rotation.eulerAngles.x);
                if (syncRotY) stream.SendNext(transform.rotation.eulerAngles.y);
                if (syncRotZ) stream.SendNext(transform.rotation.eulerAngles.z);
            }

            if (syncScale)
            {
                if (syncScaleX) stream.SendNext(transform.localScale.x);
                if (syncScaleY) stream.SendNext(transform.localScale.y);
                if (syncScaleZ) stream.SendNext(transform.localScale.z);
            }
        }
        else
        {
            // Receiving data
            Vector3 pos = transform.position;
            if (syncPosition)
            {
                if (syncPosX) pos.x = (float)stream.ReceiveNext();
                if (syncPosY) pos.y = (float)stream.ReceiveNext();
                if (syncPosZ) pos.z = (float)stream.ReceiveNext();
                networkPosition = pos;
            }

            Vector3 rot = transform.rotation.eulerAngles;
            if (syncRotation)
            {
                if (syncRotX) rot.x = (float)stream.ReceiveNext();
                if (syncRotY) rot.y = (float)stream.ReceiveNext();
                if (syncRotZ) rot.z = (float)stream.ReceiveNext();
                networkRotation = Quaternion.Euler(rot);
            }

            Vector3 scale = transform.localScale;
            if (syncScale)
            {
                if (syncScaleX) scale.x = (float)stream.ReceiveNext();
                if (syncScaleY) scale.y = (float)stream.ReceiveNext();
                if (syncScaleZ) scale.z = (float)stream.ReceiveNext();
                networkScale = scale;
            }
        }
    }
}
