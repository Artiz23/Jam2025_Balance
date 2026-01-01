using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet params")]
    [SerializeField] float speedBullet = 10f;
    [SerializeField] float damage      = 5f;

    [Header("Child visuals (оставьте пустым — возьмутся все дети)")]
    [SerializeField] Transform[] visuals;

    void Start()
    {
        ActivateSingleRandomChild();
        StartCoroutine(DestroyBullet());
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speedBullet * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out MobAI mob))
            mob.TakeDamage(damage);

        Destroy(gameObject);
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    void ActivateSingleRandomChild()
    {
        if (visuals == null || visuals.Length == 0)
        {
            int count = transform.childCount;
            visuals   = new Transform[count];
            for (int i = 0; i < count; i++)
                visuals[i] = transform.GetChild(i);
        }

        if (visuals.Length == 0) return;

        int randomIndex = Random.Range(0, visuals.Length);

        for (int i = 0; i < visuals.Length; i++)
        {
            bool isChosen = i == randomIndex;
            visuals[i].gameObject.SetActive(isChosen);

            if (isChosen)
                visuals[i].localRotation = Random.rotation;
        }
    }
}
