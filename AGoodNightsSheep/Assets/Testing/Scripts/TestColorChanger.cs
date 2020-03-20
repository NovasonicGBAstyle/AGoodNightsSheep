using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TestColorChanger : MonoBehaviour
{
    //Materials that we will be using.
    public Material greyMaterial = null;
    public Material pinkMaterial = null;

    //References to the objects we need from this
    private MeshRenderer meshRenderer = null;
    private XRGrabInteractable grabInteractable = null;

    /// <summary>
    /// This runs on awake and sets the interactables.
    /// It also sets listener events.
    /// </summary>
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.onActivate.AddListener(SetPink);
        grabInteractable.onDeactivate.AddListener(SetGrey);
    }

    /// <summary>
    /// Happens when this is destroyed.
    /// In order to keep things clean, we need to remove listeners.
    /// </summary>
    private void OnDestroy()
    {
        grabInteractable.onActivate.RemoveListener(SetPink);
        grabInteractable.onDeactivate.RemoveListener(SetGrey);
    }

    /// <summary>
    /// This will set the color of this item to the grey material.
    /// </summary>
    /// <param name="interactor"></param>
    private void SetGrey(XRBaseInteractor interactor)
    {
        meshRenderer.material = greyMaterial;
    }

    /// <summary>
    /// This will set the color of this item to the pink material.
    /// </summary>
    /// <param name="interactor"></param>
    private void SetPink(XRBaseInteractor interactor)
    {
        meshRenderer.material = pinkMaterial;
    }
}
