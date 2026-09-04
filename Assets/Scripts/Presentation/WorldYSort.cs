using UnityEngine;
using UnityEngine.Rendering;

namespace RuneMagic
{
    /// <summary>
    /// Keeps a walking body, its glow, and its nameplate together, then
    /// Y-sorts the whole group so a southern sprite draws over a northern
    /// one. Attach to every enemy and the adept so a crowded room reads.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class WorldYSort : MonoBehaviour
    {
        [SerializeField] int bias;

        SortingGroup _group;
        int _lastOrder = int.MinValue;
        Vector3 _lastPosition;

        public int Bias
        {
            get => bias;
            set => bias = value;
        }

        public static WorldYSort On(GameObject host, int bias = 0)
        {
            if (host == null)
            {
                return null;
            }

            var sort = AuthoringUtil.GetOrAdd<WorldYSort>(host, out var created);
            if (created)
            {
                sort.bias = bias;
            }

            sort.Refresh();
            return sort;
        }

        void OnEnable()
        {
            Refresh();
        }

        void LateUpdate()
        {
            if (_lastPosition == transform.position && _group != null)
            {
                return;
            }

            Refresh();
        }

        public void Refresh()
        {
            if (this == null)
            {
                return;
            }

            if (_group == null)
            {
                _group = AuthoringUtil.GetOrAdd<SortingGroup>(gameObject);
            }

            _lastPosition = transform.position;
            var order = DrawDepth.ActorOrder(_lastPosition.y, bias);
            if (order == _lastOrder && LayerMatches())
            {
                return;
            }

            DrawDepth.ApplyActor(_group, _lastPosition.y, bias);
            _lastOrder = order;
        }

        bool LayerMatches()
        {
            return _group != null &&
                (!DrawDepth.LayerExists(DrawDepth.ActorsLayer) ||
                 _group.sortingLayerName == DrawDepth.ActorsLayer);
        }
    }
}
