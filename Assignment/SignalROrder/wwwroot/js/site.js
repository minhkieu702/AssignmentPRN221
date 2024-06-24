$(() => {
    LoadProdData();
    var connection = new signalR.HubConnectionBuilder().withUrl("/signalrServer").build();
    connection.start();

    connection.on("LoadOrders", function () {
        LoadProdData();
    });

    LoadProdData();

    function LoadProdData() {
        var tr = '';
        $.ajax({
            url: '/Order/GetOrders',
            method: 'GET',
            success: (result) => {
                console.log(result); // Log the response to check its structure
                $.each(result, (k, v) => {
                    tr += `<tr>
                    <td>${v.orderId}</td>
                    <td>${v.customer.name}</td>
                    <td>${v.type}</td>
                    <td>${v.totalAmount}</td>
                    <td>${v.orderDate}</td>
                    <td>${v.orderNotes}</td>
                    <td>
                        <a href='../Order/Edit?id=${v.orderId}'>Edit</a> | 
                        <a href='../Order/Details?id=${v.orderId}'>Details</a> | 
                        <a href='../Order/Delete?id=${v.orderId}'>Delete</a>
                    </td>
                    </tr>`;
                });
                $("#tableBody").html(tr);
            },
            error: (error) => {
                console.log(error);
            }
        });
    }
});
