using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Normal.Realtime.Serialization;

public class BrushStroke : RealtimeComponent  {
     [SerializeField]
    private BrushStrokeMesh _mesh;

    // Ribbon State
    private BrushStrokeModel _model;

    // Smoothing
    private Vector3    _ribbonEndPosition;
    private Quaternion _ribbonEndRotation = Quaternion.identity;

    // Mesh
    private Vector3    _previousRibbonPointPosition;
    private Quaternion _previousRibbonPointRotation = Quaternion.identity;

    // Unity Events
    private void Update() {
        // Animate the end of the ribbon towards the brush tip
        AnimateLastRibbonPointTowardsBrushTipPosition();

        // Add a ribbon segment if the end of the ribbon has moved far enough
        AddRibbonPointIfNeeded();
    }

    // Interface
    public void BeginBrushStrokeWithBrushTipPoint(Vector3 position, Quaternion rotation) {
        // Update the model
        _model.brushTipPosition = position;
        _model.brushTipRotation = rotation;

        // Update last ribbon point to match brush tip position & rotation
        _ribbonEndPosition = position;
        _ribbonEndRotation = rotation;
        _mesh.UpdateLastRibbonPoint(_ribbonEndPosition, _ribbonEndRotation);
    }

    public void MoveBrushTipToPoint(Vector3 position, Quaternion rotation) {
        _model.brushTipPosition = position;
        _model.brushTipRotation = rotation;
    }

    public void EndBrushStrokeWithBrushTipPoint(Vector3 position, Quaternion rotation) {
        // Add a final ribbon point and mark the stroke as finalized
        AddRibbonPoint(position, rotation);
        _model.brushStrokeFinalized = true;
    }

    private BrushStrokeModel model {
        set {
            // Clear Mesh
            _mesh.ClearRibbon();

            if (_model != null) {
                // Unregister from old model events
                _model.ribbonPoints.modelAdded -= RibbonPointAdded;
            }

            _model = value;

            if (_model != null) {
                // Replace ribbon mesh
                foreach (RibbonPointModel ribbonPoint in _model.ribbonPoints)
                    _mesh.InsertRibbonPoint(ribbonPoint.position, ribbonPoint.rotation);

                // Update last ribbon point to match brush tip position & rotation
                _ribbonEndPosition = _model.brushTipPosition;
                _ribbonEndRotation = _model.brushTipRotation;
                _mesh.UpdateLastRibbonPoint(_model.brushTipPosition, _model.brushTipRotation);

                // Turn off the last ribbon point if this brush stroke is finalized
                _mesh.skipLastRibbonPoint = _model.brushStrokeFinalized;

                // Let us know when a new ribbon point is added to the mesh
                _model.ribbonPoints.modelAdded += RibbonPointAdded;
            }
        }
    }

    // Ribbon drawing
    private void AddRibbonPointIfNeeded() {
        // Only add ribbon points if this brush stroke is being drawn by the local client.
        if (!realtimeView.isOwnedLocally)
            return;

        // If the brush stroke is finalized, stop trying to add points to it.
        if (_model.brushStrokeFinalized)
            return;

        if (Vector3.Distance(_ribbonEndPosition, _previousRibbonPointPosition) >= 0.01f ||
            Quaternion.Angle(_ribbonEndRotation, _previousRibbonPointRotation) >= 10.0f) {

            // Add ribbon point model to ribbon points array. This will fire the RibbonPointAdded event to update the mesh.
            AddRibbonPoint(_ribbonEndPosition, _ribbonEndRotation);

            // Store the ribbon point position & rotation for the next time we do this calculation
            _previousRibbonPointPosition = _ribbonEndPosition;
            _previousRibbonPointRotation = _ribbonEndRotation;
        }
    }

    private void AddRibbonPoint(Vector3 position, Quaternion rotation) {
        // Create the ribbon point
        RibbonPointModel ribbonPoint = new RibbonPointModel();
        ribbonPoint.position = position;
        ribbonPoint.rotation = rotation;
        _model.ribbonPoints.Add(ribbonPoint);
    }

    private void RibbonPointAdded(RealtimeArray<RibbonPointModel> ribbonPoints, RibbonPointModel ribbonPoint, bool remote) {
        // Add ribbon point to the mesh
        _mesh.InsertRibbonPoint(ribbonPoint.position, ribbonPoint.rotation);
    }

    // Brush tip + smoothing
    private void AnimateLastRibbonPointTowardsBrushTipPosition() {
        // If the brush stroke is finalized, skip the brush tip mesh, and stop animating the brush tip.
        // if (_brushStrokeFinalized) {
        if (_model.brushStrokeFinalized) {
            _mesh.skipLastRibbonPoint = true;
            return;
        }

        Vector3    brushTipPosition = _model.brushTipPosition;
        Quaternion brushTipRotation = _model.brushTipRotation;
        // Vector3    brushTipPosition = _brushTipPosition;
        // Quaternion brushTipRotation = _brushTipRotation;

        // If the end of the ribbon has reached the brush tip position, we can bail early.
        if (Vector3.Distance(_ribbonEndPosition, brushTipPosition) <= 0.0001f &&
            Quaternion.Angle(_ribbonEndRotation, brushTipRotation) <= 0.01f) {
            return;
        }

        // Move the end of the ribbon towards the brush tip position
        _ribbonEndPosition =     Vector3.Lerp(_ribbonEndPosition, brushTipPosition, 25.0f * Time.deltaTime);
        _ribbonEndRotation = Quaternion.Slerp(_ribbonEndRotation, brushTipRotation, 25.0f * Time.deltaTime);

        // Update the end of the ribbon mesh
        _mesh.UpdateLastRibbonPoint(_ribbonEndPosition, _ribbonEndRotation);
    }
}
