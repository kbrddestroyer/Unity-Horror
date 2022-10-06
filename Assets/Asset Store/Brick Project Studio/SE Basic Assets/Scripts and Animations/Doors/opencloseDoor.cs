using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SojaExiles
{
	public class opencloseDoor : MonoBehaviour
	{
		public Animator openandclose;
		public bool open;
		public Transform Player;

		void Start()
		{
			open = false;
			Player = GameObject.FindGameObjectWithTag("Player").transform;
			Debug.Log(Player.gameObject.name);
		}

		void OnMouseOver()
		{
			Debug.Log(Player.gameObject.name);
			if (Player)
				{
					Debug.Log("1");
					float dist = Vector3.Distance(Player.position, transform.position);
					if (dist < 15)
					{
						if (open == false)
						{
							if (Input.GetKeyDown(KeyCode.E))
							{
								StartCoroutine(opening());
							}
						}
						else
						{
							if (open == true)
							{
								if (Input.GetKeyDown(KeyCode.E))
								{
									StartCoroutine(closing());
								}
							}

						}

					}
				}
		}

		IEnumerator opening()
		{
			print("you are opening the door");
			openandclose.Play("Opening");
			open = true;
			yield return new WaitForSeconds(.5f);
		}

		IEnumerator closing()
		{
			print("you are closing the door");
			openandclose.Play("Closing");
			open = false;
			yield return new WaitForSeconds(.5f);
		}


	}
}