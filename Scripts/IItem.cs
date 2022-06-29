﻿using UnityEngine;

//아이템 타입 인터페이스
public interface IItem {
    //입력으로 받는 target은 아이템 효과가 적용될 대상
    void Use(GameObject target);
}