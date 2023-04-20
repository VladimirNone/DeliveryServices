import { FC } from "react";
import DishClientCard from "./cards/DishClientCard";


const DishedList:FC<dishListProps> = ({dishes}) => {
    return (
        <>
            {dishes.map((dish, i) => <DishClientCard key={i} {...dish}/>)}
        </>
    );
}

export default DishedList;