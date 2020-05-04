using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sekiyu : MonoBehaviour
{
    private static float UpValue = 0.1f;

    [SerializeField]
    private Text _text;

    private Color _color;

    private bool isUp = false;

    private int cont = 0;

    void Start()
    {
        var force = new Vector2(Random.Range(-20000,20000),Random.Range(20000,40000));
        var rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.AddForce(force);
        _color = new Color(Random.Range(0.0f, 1.0f),Random.Range(0.0f, 1.0f),Random.Range(0.0f, 1.0f));
    }

    // Update is called once per frame
    void Update()
    {
        setColor();
        _text.color = _color;

        cont++;
        if(cont == 10){
            _color.a = 1;
        }

        if(cont > 100){
            Destroy(this.gameObject);
        }
    }

    private void setColor(){
        if(isUp) {
            if(_color.r < 1){
                _color.r += UpValue;
                return;
            }
            if(_color.g < 1){
                _color.g += UpValue;
                return;
            }
            if(_color.b < 1){
                _color.b += UpValue;
                return;
            }
            isUp = false;
        }
        if(_color.r > 0){
            _color.r -= UpValue;
            return;
        }
        if(_color.g > 0){
            _color.g -= UpValue;
            return;
        }
        if(_color.b > 0){
            _color.b -= UpValue;
            return;
        }
        isUp = true;
    }
}
