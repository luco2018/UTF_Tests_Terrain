using UnityEngine;
using System.Collections.Generic;

public class PoissonDiscInstantiator : MonoBehaviour {

	public GameObject[] prefabs;
	public float width = 100f;
	public float height = 100f;
	public float radius = 1.0f;
	public float thickness = 10;

	public bool animated = true;

	struct Instance
	{
		public GameObject go;
		public Vector3 rotationAxis;
		public float rotationSpeed;
	}

	private List<Instance> instances;

	void Start () 
	{
		Random.seed = 1111;
		int nSamples = 0;
		PoissonDiscSampler sampler = new PoissonDiscSampler(width, height, radius);
		instances = new List<Instance>();

		foreach (Vector2 sample in sampler.Samples())
		{
			int selected = Random.Range(0, prefabs.Length);
			GameObject obj = Instantiate(prefabs[selected], new Vector3(sample.x, Random.Range(0, thickness), sample.y), Random.rotation) as GameObject;

			float scaleVariance = Random.Range(1.0f, 1.5f);
			obj.transform.localScale = new Vector3(scaleVariance, scaleVariance, scaleVariance);

			MaterialPropertyBlock props = new MaterialPropertyBlock();
			float colorVarianceR = Random.Range(0.5f, 1.0f);
			float colorVarianceG = Random.Range(0.5f, 1.0f);
			float colorVarianceB = Random.Range(0.5f, 1.0f);
			props.SetColor("_Color", new Color(colorVarianceR, colorVarianceG, colorVarianceB));
			MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
			renderer.SetPropertyBlock(props);

			Instance i;
			i.go = obj;
			Random.rotation.ToAngleAxis(out i.rotationSpeed, out i.rotationAxis);
			instances.Add(i);

			nSamples++;
		}

		Debug.Log(nSamples + " rocks are spawned!");
	}

	void Update()
	{
		if (animated)
		{
			foreach (var i in instances)
				i.go.transform.Rotate(i.rotationAxis, i.rotationSpeed * Time.deltaTime);
		}
	}
}
