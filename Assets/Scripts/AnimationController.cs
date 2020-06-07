using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    Player player;
    // Start is called before the first frame update
    void Start()
    {
        player = transform.root.gameObject.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reload()
    {
        player.ammo = player.bullets;
        player.ammoText.text = "Bullets: " + player.ammo + "/30";
        if(player.soundSource2d.isActiveAndEnabled)
        player.soundSource2d.PlayOneShot(player.clips[1]);
    }

    public void Hit()
    {
        player.anim.SetBool("HitBool", false);
    }
   
}
