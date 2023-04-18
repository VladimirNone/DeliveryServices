import { FC } from "react";
import DishClientCard from "./cards/DishClientCard";


const DishedList:FC<dishListProps> = ({dishes, page}) => {
    return (
        <>
            {dishes.map((dish, i) => <DishClientCard key={i} {...dish}/>)}
        </>
    );
}

export default DishedList;