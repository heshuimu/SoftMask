﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoftMask {
    [ExecuteInEditMode]
    [AddComponentMenu("")]
    [RequireComponent(typeof(Graphic))]
    public class SoftMaskable : UIBehaviour, IMaterialModifier {
        SoftMask _mask;
        Graphic _graphic;
        bool _warned;
        Material _material;

        public Material GetModifiedMaterial(Material baseMaterial) {
            if (_mask && _mask.isActiveAndEnabled) {
                // We should find replacement first and only then Release a previous one.
                // It allows to not delete the old material if it may be reused.
                var replacement = _mask.GetReplacement(baseMaterial);
                material = replacement;
                if (material) {
                    _warned = false;
                    return material;
                }
                WarnMaskingWillNotWork(baseMaterial);
            } else {
                material = null;
            }   
            return baseMaterial;
        }
        
        public void Invalidate() {
            graphic.SetMaterialDirty();
        }

        protected override void Awake() {
            base.Awake();
            hideFlags = HideFlags.HideInInspector;
        }

        protected virtual void Update() {
            if (!mask)
                DestroyImmediate(this);
        }

        protected override void OnEnable() {
            base.OnEnable();
            FindMask();
        }

        protected override void OnDisable() {
            base.OnDisable();
            mask = null;
        }

        protected override void OnTransformParentChanged() {
            base.OnTransformParentChanged();
            FindMask();
        }

        Graphic graphic { get { return _graphic ?? (_graphic = GetComponent<Graphic>()); } }

        Material material {
            get { return _material; }
            set {
                if (_material != value) {
                    if (_material && _mask)
                        _mask.ReleaseReplacement(_material);
                    _material = value;
                }
            }
        }

        SoftMask mask {
            get { return _mask; }
            set {
                if (_mask != value) {
                    if (_mask)
                        material = null;
                    _mask = value;
                    Invalidate();
                }
            }
        }

        void FindMask() {
            mask = GetComponentInParent<SoftMask>();
            
        }

        void WarnMaskingWillNotWork(Material material) {
            if (!_warned) {
                Debug.LogWarningFormat(
                    gameObject,
                    "Soft Mask will not work on {0} because material {1} doesn't support masking. " +
                    "Add masking support to your material or set Graphic's material to None to use " +
                    "a default one.",
                    graphic,
                    material);
                _warned = true;
            }
        }
    }
}