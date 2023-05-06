import React, { FC } from 'react';
import CounterMainDish from './components/CounterMainDish';
import DishStandartLayout from '../structure/DishStandartLayout';

const DishMainCard: FC<dishClientInfo> = (dishInfo) => {

    return (
        <DishStandartLayout dishInfo={dishInfo}>
            <CounterMainDish dishId={dishInfo.id}/>
        </DishStandartLayout>
    );
}

export default DishMainCard;

