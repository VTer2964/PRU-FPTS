using UnityEngine;
using Unity.Cinemachine; // Nhớ có chữ Unity. ở đầu

public class Camera_Main_Menu : MonoBehaviour
{
    // Ở bản mới, tên class là CinemachineCamera, không phải CinemachineVirtualCamera
    public CinemachineCamera menuCam;
    public CinemachineCamera settingsCam;

    public void OpenSettings()
    {
        Debug.Log("Da bam nut Mo Settings!");

        if (menuCam != null && settingsCam != null)
        {
            menuCam.Priority = 10;
            settingsCam.Priority = 20;
            Debug.Log("Da doi Priority thanh cong!");
        }
        else
        {
            Debug.LogError("Loi: Ban chua keo Camera vao o trong cua Script roi!");
        }
    }

    public void OpenMenu()
    {
        if (menuCam != null && settingsCam != null)
        {
            menuCam.Priority = 20;
            settingsCam.Priority = 10;
        }
    }
}