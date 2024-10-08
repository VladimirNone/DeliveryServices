import React, { FC } from 'react';
import CounterCartDish from './components/CounterCartDish';
import DishStandartLayout from '../structure/DishStandartLayout';

const DishCartCard: FC<dishCartInfo> = (dishInfo) => {

    return (
        <DishStandartLayout dishInfo={dishInfo}>
            <CounterCartDish dishId={dishInfo.id} cancelDish={dishInfo.DeleteCardFromList}/>
        </DishStandartLayout>
    );
}

export default DishCartCard;

