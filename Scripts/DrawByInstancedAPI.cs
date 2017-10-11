using UnityEngine;
using System.Collections.Generic;

public class DrawByInstancedAPI : MonoBehaviour {

	public Mesh[] meshes = null;
	public Material material = null;

	public float width = 100f;
	public float height = 100f;
	public float radius = 1.0f;
	public float thickness = 10;

	public bool animated = true;

	class Instance
	{
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scaling;

		public Vector3 rotationAxis;
		public float rotationSpeed;
	}

	class MeshInstances
	{
		public Mesh mesh;
		public List<Instance> instances;
		public MaterialPropertyBlock props;
	}

	List<MeshInstances> instances;

	// Use this for initialization
	void Start()
	{
		Random.InitState(1111);
		int nSamples = 0;
		PoissonDiscSampler sampler = new PoissonDiscSampler(width, height, radius);

		instances = new List<MeshInstances>(meshes.Length);
		for (int i = 0; i < meshes.Length; ++i)
		{
			MeshInstances meshInstances = new MeshInstances();
			meshInstances.mesh = meshes[i];
			meshInstances.instances = new List<Instance>();
			meshInstances.props = new MaterialPropertyBlock();
			instances.Add(meshInstances);
		}

		foreach (Vector2 sample in sampler.Samples())
		{
			++nSamples;

			int selected = Random.Range(0, meshes.Length);

			Instance i = new Instance();
			i.position = new Vector3(sample.x, Random.Range(0, thickness), sample.y);
			i.rotation = Random.rotation;
			i.scaling = Vector3.one;
			Random.rotation.ToAngleAxis(out i.rotationSpeed, out i.rotationAxis);

			instances[selected].instances.Add(i);
		}

		foreach (var meshInstances in instances)
		{
			//int numColors = Random.Range(0, meshInstances.instances.Count);
			//int numColors = 30;
			int numColors = meshInstances.instances.Count;
			Vector4[] colors = new Vector4[numColors];
			for (int i = 0; i < numColors; ++i)
			{
				float colorVarianceR = Random.Range(0.5f, 1.0f);
				float colorVarianceG = Random.Range(0.5f, 1.0f);
				float colorVarianceB = Random.Range(0.5f, 1.0f);
				colors[i] = new Color(colorVarianceR, colorVarianceG, colorVarianceB);
			}
			meshInstances.props.SetVectorArray("_Color", colors);
		}

		Debug.Log(nSamples + " rocks are spawned!");
	}

	// Update is called once per frame
	void Update ()
	{
		foreach (var meshInstance in instances)
		{
			Matrix4x4[] matrices = new Matrix4x4[meshInstance.instances.Count];
			for (int i = 0; i < meshInstance.instances.Count; ++i)
			{
				var inst = meshInstance.instances[i];
				if (animated)
					inst.rotation = inst.rotation * Quaternion.AngleAxis(inst.rotationSpeed * Time.deltaTime, inst.rotationAxis);
				matrices[i] = Matrix4x4.TRS(inst.position, inst.rotation, inst.scaling);
			}

			Graphics.DrawMeshInstanced(meshInstance.mesh, 0, material, matrices, matrices.Length, meshInstance.props);
		}
	}
}
