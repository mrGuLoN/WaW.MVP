using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

public class DestroyWithoutMeshFilter : MonoBehaviour
{
   [SerializeField] private Transform _parent;
   

   [ProButton]
   public void SetMesh()
   {
	   var meshes = GetComponentsInChildren<MeshFilter>();
	   foreach (var m in meshes)
	   {
		   m.transform.SetParent(_parent);
	   }
   }
}
